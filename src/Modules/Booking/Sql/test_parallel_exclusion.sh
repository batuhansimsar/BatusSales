#!/usr/bin/env bash
# Verify that two concurrent PostgreSQL clients cannot reserve the same staff slot.
# CI-only: run against a disposable DB after 001_appointments.sql.
set -euo pipefail

case "$PGDATABASE" in
  booking_ci|booking_test) ;;
  *) echo "Refusing to run booking tests against a non-test database" >&2; exit 1 ;;
esac

: "$PGPASSWORD"
export PGHOST=127.0.0.1 PGPORT=5432 PGUSER=postgres

workdir="$(mktemp -d)"
export SYNC_DIR="$workdir"
holder_pid=""
challenger_pid=""

cleanup() {
  touch "$workdir/release"
  if [[ -n "$holder_pid" ]]; then wait "$holder_pid" >/dev/null 2>&1 || true; fi
  if [[ -n "$challenger_pid" ]]; then wait "$challenger_pid" >/dev/null 2>&1 || true; fi
  psql -X -v ON_ERROR_STOP=1 -q -c "
    DELETE FROM booking.appointments
    WHERE tenant_id = '10000000-0000-0000-0000-000000000099'
      AND id IN (
        '00000000-0000-0000-0000-000000000091',
        '00000000-0000-0000-0000-000000000092'
      );" >/dev/null 2>&1 || true
  rm -rf "$workdir"
}
trap cleanup EXIT

cat > "$workdir/holder.sql" <<'SQL'
BEGIN;
INSERT INTO booking.appointments
  (id, tenant_id, staff_id, starts_at, ends_at, status)
VALUES
  ('00000000-0000-0000-0000-000000000091',
   '10000000-0000-0000-0000-000000000099',
   '20000000-0000-0000-0000-000000000099',
   '2026-10-22 13:00:00+00', '2026-10-22 13:30:00+00', 'confirmed');
\echo HOLDER_READY
\! while [ ! -f "$SYNC_DIR/release" ]; do sleep 0.1; done
COMMIT;
SQL

psql -X -v ON_ERROR_STOP=1 -f "$workdir/holder.sql" >"$workdir/holder.log" 2>&1 &
holder_pid=$!

holder_ready=0
for ((i=0; i<150; i++)); do
  if grep -q 'HOLDER_READY' "$workdir/holder.log"; then holder_ready=1; break; fi
  if ! kill -0 "$holder_pid" 2>/dev/null; then break; fi
  sleep 0.1
done
if [[ "$holder_ready" -ne 1 ]]; then
  echo "First booking never reached the uncommitted state" >&2
  cat "$workdir/holder.log" >&2
  exit 1
fi

# The second independent connection tries to reserve an overlapping interval.
PGAPPNAME=booking_parallel_challenger PGOPTIONS='-c statement_timeout=15000' \
  psql -X -v ON_ERROR_STOP=1 -v VERBOSITY=verbose -c "
    INSERT INTO booking.appointments
      (id, tenant_id, staff_id, starts_at, ends_at, status)
    VALUES
      ('00000000-0000-0000-0000-000000000092',
       '10000000-0000-0000-0000-000000000099',
       '20000000-0000-0000-0000-000000000099',
       '2026-10-22 13:15:00+00', '2026-10-22 13:45:00+00', 'pending');" \
  >"$workdir/challenger.log" 2>&1 &
challenger_pid=$!

# Confirm PostgreSQL is actually blocking B while A is uncommitted.
observed_lock=0
for ((i=0; i<100; i++)); do
  blocked="$(psql -X -At -v ON_ERROR_STOP=1 -c "
    SELECT count(*) FROM pg_stat_activity
    WHERE application_name = 'booking_parallel_challenger'
      AND wait_event_type = 'Lock';")"
  if [[ "$blocked" != "0" ]]; then observed_lock=1; break; fi
  if ! kill -0 "$challenger_pid" 2>/dev/null; then break; fi
  sleep 0.1
done

touch "$workdir/release"
if ! wait "$holder_pid"; then
  echo "First reservation failed" >&2
  cat "$workdir/holder.log" >&2
  exit 1
fi
holder_pid=""

if wait "$challenger_pid"; then
  echo "Double booking unexpectedly succeeded" >&2
  exit 1
fi
challenger_pid=""

if [[ "$observed_lock" -ne 1 ]]; then
  echo "Did not observe a blocked concurrent reservation" >&2
  cat "$workdir/challenger.log" >&2
  exit 1
fi
if ! grep -Eq '23P01:.*booking_appointments_staff_no_overlap' "$workdir/challenger.log"; then
  echo "Challenger failed for a reason other than an exclusion violation" >&2
  cat "$workdir/challenger.log" >&2
  exit 1
fi

committed="$(psql -X -At -v ON_ERROR_STOP=1 -c "
  SELECT count(*) FROM booking.appointments
  WHERE tenant_id = '10000000-0000-0000-0000-000000000099'
    AND id IN (
      '00000000-0000-0000-0000-000000000091',
      '00000000-0000-0000-0000-000000000092'
    );")"
if [[ "$committed" != "1" ]]; then
  echo "Expected one committed appointment; got $committed" >&2
  exit 1
fi

echo "PASS: concurrent inserts cannot double-book a tenant/staff slot"
