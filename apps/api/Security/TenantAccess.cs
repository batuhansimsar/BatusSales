using System.Security.Claims;
using BatuSales.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BatuSales.Api.Security;

/// <summary>Checks database membership before enabling tenant-scoped queries.</summary>
public sealed class TenantAccess(BatuSalesDbContext db, TenantScope scope)
{
    public async Task<bool> TryEnterAsync(Guid organizationId, ClaimsPrincipal user, CancellationToken ct)
    {
        var subject = user.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(subject) || organizationId == Guid.Empty)
            return false;

        var isMember = await db.Members
            .AsNoTracking()
            .AnyAsync(m => m.OrganizationId == organizationId && m.UserId == subject, ct);

        if (isMember)
            scope.Enter(organizationId);

        return isMember;
    }
}
