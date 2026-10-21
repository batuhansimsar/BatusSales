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
        return TryGetUserId(user, out var userId)
            && await db.Memberships.AsNoTracking().AnyAsync(
                x => x.OrganizationId == organizationId && x.UserId == userId, ct);
    }
}
