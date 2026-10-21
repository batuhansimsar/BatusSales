using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Features.Audit;

/// <summary>
/// Organization audit history. Visibility is restricted to current Owner/Admin
/// memberships; the caller cannot select a different tenant by changing the URL.
/// </summary>
public static class AuditEndpoints
{
    public sealed record AuditEventResponse(
        Guid Id, Guid OrganizationId, Guid ActorId,
        string Action, Guid EntityId, DateTime CreatedAtUtc);

    public static void MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations/{organizationId:guid}/audit-events")
            .RequireAuthorization();

        group.MapGet("", async (
            Guid organizationId, int? page, int? pageSize, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            // Verify membership and role from PostgreSQL on every request.
            // Non-members receive 404 to avoid exposing organization existence.
            var role = await TenantAccess.GetRoleAsync(db, context.User, organizationId, ct);
            if (role is null) return Results.NotFound();
            if (role is not (OrganizationRole.Owner or OrganizationRole.Admin))
                return Results.Forbid();

            var currentPage = Math.Clamp(page ?? 1, 1, 100_000);
            var size = Math.Clamp(pageSize ?? 25, 1, 100);

            // Explicit tenant predicate also protects audit rows, which do not
            // currently have a tenant-level EF query filter.
            var query = db.AuditEvents.AsNoTracking()
                .Where(e => e.OrganizationId == organizationId);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(e => e.CreatedAtUtc)
                .ThenByDescending(e => e.Id)
                .Skip((currentPage - 1) * size).Take(size)
                .Select(e => new AuditEventResponse(
                    e.Id, e.OrganizationId, e.ActorId, e.Action,
                    e.EntityId, e.CreatedAtUtc))
                .ToListAsync(ct);

            // No arbitrary text, PII, request bodies or tokens in the response.
            return Results.Ok(new { items, page = currentPage, pageSize = size, total });
        });
    }
}
