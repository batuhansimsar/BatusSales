using System.Data.Common;
using BatusSales.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Infrastructure;

/// <summary>
/// Tests that baseline Identity, tenancy, CRM, and audit schema can actually be
/// queried. CanConnect alone reports healthy for an empty PostgreSQL database.
/// This is a lightweight P0 safety probe, not a substitute for EF migrations,
/// migration-state checks or a release-time migration verification.
/// </summary>
public static class DatabaseReadiness
{
    public static async Task<bool> IsReadyAsync(AppDbContext db, CancellationToken ct)
    {
        if (!await db.Database.CanConnectAsync(ct))
            return false;

        try
        {
            // AnyAsync sends EXISTS probes, loading no user/contact records.
            // Fail closed if a required relation was never created or was removed.
            _ = await db.Users.AsNoTracking().AnyAsync(ct);
            _ = await db.Organizations.AsNoTracking().AnyAsync(ct);
            _ = await db.Memberships.AsNoTracking().AnyAsync(ct);
            // The verified tenant is intentionally absent in an infrastructure
            // probe. Ignoring tenant/soft-delete filters here prevents an EF
            // predicate from constant-folding the table check away. These
            // queries return only bools; they never expose customer rows.
            _ = await db.Contacts.IgnoreQueryFilters().AsNoTracking().AnyAsync(ct);
            _ = await db.AuditEvents.IgnoreQueryFilters().AsNoTracking().AnyAsync(ct);
            return true;
        }
        catch (DbException)
        {
            // The public health route must not leak SQL errors or schema details.
            return false;
        }
    }
}
