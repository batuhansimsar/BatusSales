using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Features.Organizations;

public static class OrganizationEndpoints
{
    public sealed record CreateOrganizationRequest(string? Name);
    public sealed record OrganizationResponse(Guid Id, string Name, OrganizationRole Role);

    public static void MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations").RequireAuthorization("BatusSalesBearer");

        group.MapPost("", async (
            CreateOrganizationRequest input, HttpContext context, AppDbContext db, CancellationToken ct) =>
        {
            if (!TenantAccess.TryGetUserId(context.User, out var userId))
                return Results.Unauthorized();

            var name = input.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["Organization name must be between 1 and 150 characters."]
                });

            var org = new Organization { Name = name };
            db.Organizations.Add(org);
            db.Memberships.Add(new OrganizationMembership
            {
                OrganizationId = org.Id, UserId = userId, Role = OrganizationRole.Owner
            });
            db.AuditEvents.Add(new AuditEvent
            {
                OrganizationId = org.Id, ActorId = userId,
                Action = "organization.created", EntityId = org.Id
            });
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/organizations/{org.Id}",
                new OrganizationResponse(org.Id, org.Name, OrganizationRole.Owner));
        });

        group.MapGet("", async (HttpContext context, AppDbContext db, CancellationToken ct) =>
        {
            if (!TenantAccess.TryGetUserId(context.User, out var userId))
                return Results.Unauthorized();

            var organizations = await (
                from membership in db.Memberships.AsNoTracking()
                join organization in db.Organizations.AsNoTracking()
                    on membership.OrganizationId equals organization.Id
                where membership.UserId == userId
                orderby organization.Name
                select new OrganizationResponse(organization.Id, organization.Name, membership.Role)
            ).ToListAsync(ct);

            return Results.Ok(organizations);
        });
    }
}
