using System.Security.Claims;

namespace BatusSales.Api.Auth;

public sealed class TenantContext(IHttpContextAccessor httpContextAccessor)
{
    public Guid? TenantId => ReadTenantId(httpContextAccessor.HttpContext?.User);

    public static Guid? ReadTenantId(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var raw = user.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(raw, out var id) && id != Guid.Empty
            ? id
            : null;
    }
}
