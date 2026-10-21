using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Security;

/// <summary>
/// All organization-scoped routes must include this filter.
/// A client-provided organization ID is never sufficient proof of authorization.
/// </summary>
public sealed class TenantAccessFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        if (!Guid.TryParse(http.Request.RouteValues["organizationId"]?.ToString(),
                out var organizationId))
        {
            return Results.BadRequest(new { error = "Invalid organization ID." });
        }

        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var db = http.RequestServices.GetRequiredService<SalesDbContext>();
        var member = await db.Memberships.AsNoTracking()
            .AnyAsync(m => m.OrganizationId == organizationId && m.UserId == userId,
                http.RequestAborted);

        if (!member)
        {
            // Respond with 404 so the caller cannot enumerate other organizations.
            return Results.NotFound();
        }

        http.RequestServices.GetRequiredService<CurrentTenant>().Id = organizationId;
        return await next(context);
    }
}
