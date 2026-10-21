using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Infrastructure;

public static class TenantAccess
{
    public static bool TryGetUserId(ClaimsPrincipal user, out Guid userId) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    public static async Task<bool> IsMemberAsync(
        AppDbContext db, ClaimsPrincipal user, Guid organizationId, CancellationToken ct)
    {
        db.ClearAuthorizedTenant();
        if (organizationId == Guid.Empty || !TryGetUserId(user, out var userId))
            return false;

        var member = await db.Memberships.AsNoTracking().AnyAsync(
            x => x.OrganizationId == organizationId && x.UserId == userId, ct);
        if (member)
            db.SetAuthorizedTenant(organizationId);

        return member;
    }

    /// <summary>Resolve a role from database membership, never from client-supplied claims.</summary>
    public static async Task<OrganizationRole?> GetRoleAsync(
        AppDbContext db, ClaimsPrincipal user, Guid organizationId, CancellationToken ct)
    {
        db.ClearAuthorizedTenant();
        if (organizationId == Guid.Empty || !TryGetUserId(user, out var userId))
            return null;

        var role = await db.Memberships.AsNoTracking()
            .Where(m => m.OrganizationId == organizationId && m.UserId == userId)
            .Select(m => (OrganizationRole?)m.Role)
            .SingleOrDefaultAsync(ct);

        if (role.HasValue)
            db.SetAuthorizedTenant(organizationId);

        return role;
    }

    /// <summary>Only owners and admins may soft-delete contact records.</summary>
    public static bool CanDeleteContacts(OrganizationRole? role) =>
        role is OrganizationRole.Owner or OrganizationRole.Admin;
}
