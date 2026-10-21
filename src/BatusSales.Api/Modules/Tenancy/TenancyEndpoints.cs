using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Modules.Tenancy;

public static class TenancyEndpoints
{
    public static IEndpointRouteBuilder MapTenancyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/organizations").RequireAuthorization();
        group.MapPost("", CreateOrganization);
        group.MapGet("", ListOrganizations);
        return endpoints;
    }

    private static async Task<IResult> CreateOrganization(
        CreateOrganizationRequest input,
        ClaimsPrincipal user,
        SalesDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var name = input.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 160)
            return Results.BadRequest(new { error = "Organization name must be 1-160 characters." });

        var organization = new Organization { Name = name };
        db.Organizations.Add(organization);
        db.Memberships.Add(new Membership
        {
            OrganizationId = organization.Id,
            UserId = userId,
            Role = "Owner"
        });
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/organizations/{organization.Id}",
            new OrganizationResponse(organization.Id, organization.Name, "Owner"));
    }

    private static async Task<IResult> ListOrganizations(
        ClaimsPrincipal user,
        SalesDbContext db,
        CancellationToken cancellationToken)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var organizations = await (
            from member in db.Memberships.AsNoTracking()
            join organization in db.Organizations.AsNoTracking()
                on member.OrganizationId equals organization.Id
            where member.UserId == userId
            orderby organization.Name
            select new OrganizationResponse(
                organization.Id, organization.Name, member.Role))
            .ToListAsync(cancellationToken);

        return Results.Ok(organizations);
    }

    public sealed record CreateOrganizationRequest(string? Name);
    public sealed record OrganizationResponse(Guid Id, string Name, string Role);
}
