using System.Security.Claims;
using BatusSales.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Security;

public static class TenantAccess
{
    public static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static Task<OrganizationMembership?> GetMembershipAsync(
        AppDbContext db,
        Guid organizationId,
        string userId,
        CancellationToken cancellationToken) =>
        db.OrganizationMemberships.AsNoTracking().SingleOrDefaultAsync(
            membership => membership.OrganizationId == organizationId && membership.UserId == userId,
            cancellationToken);
}
