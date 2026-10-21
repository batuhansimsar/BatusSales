# Booking P0 domain rules

This module is a **pure .NET 10 domain foundation**, not a deployed booking API.

It provides:
- Immutable appointments with nonempty tenant/staff/appointment IDs
- UTC-normalized instants and positive time intervals
- Basic pending/confirmed/cancelled states
- Tenant- and staff-scoped interval-overlap preflight
- Self-reschedule and cancellation behavior
- Dependency-free executable checks

Run the executable checks from the repository root:

```bash
dotnet run --project src/Modules/Booking/BatusSales.Booking.Checks/BatusSales.Booking.Checks.csproj
```

**Important:** The overlap check is not transaction-safe. Before any reservation API is exposed, persist bookings in PostgreSQL with an exclusion constraint or equivalent transactional protection. Also add authenticated tenant resolution from claims/membership, role checks, idempotency, service/staff hours, time zone and daylight-saving handling, privacy/audit records, and integration tests. Do not use X-Tenant-ID as an authorization source.

No external NuGet dependency is introduced by this increment.
