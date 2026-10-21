# Booking P0 domain rules

This module is a **pure .NET 10 booking-domain foundation with a tested PostgreSQL schema contract**, not a deployed booking API.

It provides:
- Immutable appointments with nonempty tenant/staff/appointment IDs
- UTC-normalized instants and positive time intervals
- Basic pending/confirmed/cancelled states
- Tenant- and staff-scoped interval-overlap preflight
- Self-reschedule and cancellation behavior
- Dependency-free executable checks
- PostgreSQL GiST exclusion constraint preventing overlapping active reservations, also on concurrent inserts/updates
- SQL regression scenarios for touching intervals, cross-tenant/staff independence, cancellation, reactivation and invalid duration

Domain checks from the repository root:

```bash
dotnet run --project src/Modules/Booking/BatusSales.Booking.Checks/BatusSales.Booking.Checks.csproj
```

SQL tests must run **only against a disposable empty PostgreSQL 17+ database**. Schema DDL is a reference for the future canonical migration, not a production upgrade mechanism:

```bash
psql -X -v ON_ERROR_STOP=1 -d booking_test \
  -f src/Modules/Booking/Sql/001_appointments.sql \
  -f src/Modules/Booking/Sql/test_appointments.sql
```

GitHub Actions runs both sets of checks against PostgreSQL 17. The SQL regression script rolls back test inserts, but **the schema script creates persistent tables**. Never aim these commands at a customer database.

**Still required before a booking API can launch:** integration into the selected Identity/Tenancy backend and real EF migrations; authenticated tenant membership and RBAC; service/staff working hours, holidays and business time zones (DST); request idempotency, cancellation policy and audit; parallel-client authenticated integration tests. The SQL contract has no organization/staff foreign keys because the canonical schema has not been chosen.

Do not use client-supplied X-Tenant-ID as authorization. No external NuGet dependency is introduced.
