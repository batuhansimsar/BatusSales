using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BatusSales.Api.Features.Organizations;

/// <summary>Read-only membership directory; invitation and role changes remain separate P0 work.</summary>
public static class OrganizationMemberEndpoints
{
    public sealed record MemberResponse(Guid UserId, string? Email, OrganizationRole Role);

    public static void MapOrganizationMemberEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/organizations/{organizationId:guid}/members", async (
            Guid organizationId, HttpContext context, AppDbContext db, CancellationToken ct) =>
        {
            // Recheck live database membership on every request, never trust a token's stale role.
            var role = await TenantAccess.GetRoleAsync(db, context.User, organizationId, ct);
            if (role is null)
                return Results.NotFound(); // Conceal the existence of unrelated organizations.
            if (role is not (OrganizationRole.Owner or OrganizationRole.Admin))
                return Results.Forbid();

            var members = await (
                from member in db.Memberships.AsNoTracking()
                join user in db.Users.AsNoTracking() on member.UserId equals user.Id
                where member.OrganizationId == organizationId
                orderby user.Email, member.UserId
                select new MemberResponse(member.UserId, user.Email, member.Role)
            ).ToListAsync(ct);

            return Results.Ok(members);
        }).RequireAuthorization(new AuthorizeAttribute
        {
            AuthenticationSchemes = IdentityConstants.BearerScheme
        });
    }
}
