# BatuSales development log — Booking domain increment

Date: 2026-10-21
Scope: P0 Booking — narrow domain rules
Branch: feat/p0-booking-domain-rules-20261021

## Repository baseline

The initial review found an empty repo. During the review, a different GitHub session created an `Initial commit` and several foundation branches. This increment was deliberately kept separate from concurrent identity, tenancy and CRM efforts.

## Included in this PR

- Pure Booking domain project, independent of HTTP and storage.
- Appointment validation, UTC normalization and cancellation.
- Availability preflight, scoped by tenant and staff with half-open intervals.
- Self-reschedule exclusion and checks covering collision and isolation behavior.
- Explanation that DB-level uniqueness/concurrency and authorization are not implemented.

## Not implemented / never claim completed

- Authentication and tenant authorization
- PostgreSQL persistence, transaction-safe overlap exclusion, migrations
- HTTP booking endpoints and UI
- Service hours, holidays, timezone/DST scheduling, reminders, audit
- CI and runtime build verification (this execution environment has no .NET SDK)

## Next steps

1. Reconcile this module with the foundation PR that is actually merged first.
2. Add a tenant-authorized persistence adapter and PostgreSQL exclusion rule.
3. Test parallel concurrent inserts against a real database.
4. Wire calendar APIs only after authorization and idempotency checks.

## Technology review

- .NET 10 is active LTS as of October 2026.
- `DateTimeOffset` is used for unambiguous instants; a named time zone is still needed for business-hours rules.
- PostgreSQL `btree_gist` + a GiST exclusion constraint should enforce the overlap invariant for concurrent writes.
