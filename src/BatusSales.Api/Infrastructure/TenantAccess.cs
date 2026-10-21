using System.Security.Claims;
using BatusSales.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Infrastructure;

public static class TenantAccess
{
    public static bool TryGetUserId(ClaimsPrincipal user, out Guid userId) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    public static async Task<bool> IsMemberAsync(
        AppDbContext db, ClaimsPrincipal user, Guid organizationId, CancellationToken ct)
    {
        // Never keep a previously authorized tenant after a failed membership check.
        db.ClearTenant();
        if (!TryGetUserId(user, out var userId))
            return false;

        var isMember = await db.Memberships.AsNoTracking().AnyAsync(
            x => x.OrganizationId == organizationId && x.UserId == userId, ct);
        if (isMember)
            db.ActivateTenant(organizationId);
        return isMember;
    }
}
