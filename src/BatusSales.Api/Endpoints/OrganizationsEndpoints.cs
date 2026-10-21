using BatusSales.Api.Data;
using BatusSales.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Endpoints;

public static class OrganizationsEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations").RequireAuthorization();

        group.MapPost("/", async (CreateOrganizationRequest input, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var userId = TenantAccess.GetUserId(http.User);
            if (userId is null) return Results.Unauthorized();

            var name = input.Name?.Trim();
            if (string.IsNullOrEmpty(name) || name.Length > 120)
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["Business name is required and must be 120 characters or fewer."]
                });

            var organization = new Organization { Name = name, CreatedByUserId = userId };
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            db.Organizations.Add(organization);
            db.OrganizationMemberships.Add(new OrganizationMembership
            {
                OrganizationId = organization.Id,
                UserId = userId,
                Role = OrganizationRoles.Owner
            });
            db.AuditEvents.Add(new AuditEvent
            {
                OrganizationId = organization.Id,
                ActorUserId = userId,
                Action = "organization.created",
                EntityId = organization.Id
            });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return Results.Created($"/api/organizations/{organization.Id}",
                new OrganizationResponse(organization.Id, organization.Name, OrganizationRoles.Owner));
        });

        group.MapGet("/", async (HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var userId = TenantAccess.GetUserId(http.User);
            if (userId is null) return Results.Unauthorized();

            var organizations = await (
                from membership in db.OrganizationMemberships.AsNoTracking()
                join organization in db.Organizations.AsNoTracking() on membership.OrganizationId equals organization.Id
                where membership.UserId == userId
                orderby organization.CreatedAt descending
                select new OrganizationResponse(organization.Id, organization.Name, membership.Role)
            ).ToListAsync(ct);
            return Results.Ok(organizations);
        });

        group.MapGet("/{organizationId:guid}", async (Guid organizationId, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var userId = TenantAccess.GetUserId(http.User);
            if (userId is null) return Results.Unauthorized();

            var organization = await (
                from membership in db.OrganizationMemberships.AsNoTracking()
                join org in db.Organizations.AsNoTracking() on membership.OrganizationId equals org.Id
                where membership.UserId == userId && membership.OrganizationId == organizationId
                select new OrganizationResponse(org.Id, org.Name, membership.Role)
            ).SingleOrDefaultAsync(ct);
            return organization is null ? Results.NotFound() : Results.Ok(organization);
        });

        return app;
    }

    public sealed record CreateOrganizationRequest(string? Name);
    public sealed record OrganizationResponse(Guid Id, string Name, string Role);
}
