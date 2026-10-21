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
        // Never keep a previously authorized tenant after a failed membership check.
        db.ClearTenant();
        if (organizationId == Guid.Empty || !TryGetUserId(user, out var userId))
            return false;

        var isMember = await db.Memberships.AsNoTracking().AnyAsync(
            x => x.OrganizationId == organizationId && x.UserId == userId, ct);
        if (isMember)
            db.ActivateTenant(organizationId);
        return isMember;
    }

    /// <summary>Load the role from the current user's database membership, not request claims.</summary>
    public static async Task<OrganizationRole?> GetRoleAsync(
        AppDbContext db, ClaimsPrincipal user, Guid organizationId, CancellationToken ct)
    {
        db.ClearTenant();
        if (organizationId == Guid.Empty || !TryGetUserId(user, out var userId))
            return null;

        var role = await db.Memberships.AsNoTracking()
            .Where(m => m.OrganizationId == organizationId && m.UserId == userId)
            .Select(m => (OrganizationRole?)m.Role)
            .SingleOrDefaultAsync(ct);
        if (role.HasValue)
            db.ActivateTenant(organizationId);
        return role;
    }

    /// <summary>Soft-delete of CRM contacts is restricted to Owner and Admin roles.</summary>
    public static bool CanDeleteContacts(OrganizationRole? role) =>
        role is OrganizationRole.Owner or OrganizationRole.Admin;

}
