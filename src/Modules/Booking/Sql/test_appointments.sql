-- Run only on an isolated CI database after 001_appointments.sql.
-- ON_ERROR_STOP=1 must be supplied to psql. This test rolls back all rows.
BEGIN;

INSERT INTO booking.appointments (id, tenant_id, staff_id, starts_at, ends_at, status) VALUES
('00000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000001','20000000-0000-0000-0000-000000000001','2026-10-22 09:00:00+00','2026-10-22 09:30:00+00','confirmed');

-- Equal / partially overlapping slots for the same tenant and staff MUST fail.
DO $assert$
BEGIN
    BEGIN
        INSERT INTO booking.appointments VALUES
        ('00000000-0000-0000-0000-000000000002','10000000-0000-0000-0000-000000000001','20000000-0000-0000-0000-000000000001','2026-10-22 09:15:00+00','2026-10-22 09:45:00+00','pending');
        RAISE EXCEPTION 'Expected exclusion violation for overlapping appointments';
    EXCEPTION WHEN exclusion_violation THEN
        NULL;
    END;
END $assert$;

-- End-exclusive boundaries: back-to-back bookings are legal.
INSERT INTO booking.appointments VALUES
('00000000-0000-0000-0000-000000000003','10000000-0000-0000-0000-000000000001','20000000-0000-0000-0000-000000000001','2026-10-22 09:30:00+00','2026-10-22 10:00:00+00','confirmed');

-- Staff identifiers are scoped to a tenant: a different tenant is independent.
INSERT INTO booking.appointments VALUES
('00000000-0000-0000-0000-000000000004','10000000-0000-0000-0000-000000000002','20000000-0000-0000-0000-000000000001','2026-10-22 09:00:00+00','2026-10-22 09:30:00+00','pending');

-- A different staff member in the same tenant is independent.
INSERT INTO booking.appointments VALUES
('00000000-0000-0000-0000-000000000005','10000000-0000-0000-0000-000000000001','20000000-0000-0000-0000-000000000002','2026-10-22 09:00:00+00','2026-10-22 09:30:00+00','pending');

-- Cancelled rows never block a slot.
INSERT INTO booking.appointments VALUES
('00000000-0000-0000-0000-000000000006','10000000-0000-0000-0000-000000000001','20000000-0000-0000-0000-000000000001','2026-10-22 09:05:00+00','2026-10-22 09:20:00+00','cancelled');

-- Cancelling a prior reservation releases the slot; reactivating it when
-- the slot has been taken MUST fail, including on UPDATE.
UPDATE booking.appointments SET status = 'cancelled'
WHERE tenant_id = '10000000-0000-0000-0000-000000000001'
  AND id = '00000000-0000-0000-0000-000000000001';

INSERT INTO booking.appointments VALUES
('00000000-0000-0000-0000-000000000007','10000000-0000-0000-0000-000000000001','20000000-0000-0000-0000-000000000001','2026-10-22 09:05:00+00','2026-10-22 09:20:00+00','confirmed');

DO $assert$
BEGIN
    BEGIN
        UPDATE booking.appointments SET status = 'confirmed'
        WHERE tenant_id = '10000000-0000-0000-0000-000000000001'
          AND id = '00000000-0000-0000-0000-000000000001';
        RAISE EXCEPTION 'Expected exclusion violation on reactivation';
    EXCEPTION WHEN exclusion_violation THEN
        NULL;
    END;
END $assert$;

-- Reject zero-duration reservations at the database boundary.
DO $assert$
BEGIN
    BEGIN
        INSERT INTO booking.appointments VALUES
        ('00000000-0000-0000-0000-000000000008','10000000-0000-0000-0000-000000000001','20000000-0000-0000-0000-000000000001','2026-10-22 11:00:00+00','2026-10-22 11:00:00+00','pending');
        RAISE EXCEPTION 'Expected duration check violation';
    EXCEPTION WHEN check_violation THEN
        NULL;
    END;
END $assert$;

DO $assert$
DECLARE actual_count integer;
BEGIN
    SELECT count(*) INTO actual_count FROM booking.appointments;
    IF actual_count <> 6 THEN
        RAISE EXCEPTION 'Expected 6 nonrejected rows, got %', actual_count;
    END IF;
END $assert$;

ROLLBACK;
SELECT 'PASS: tenant/staff booking exclusion invariants' AS result;
