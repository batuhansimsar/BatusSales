using Microsoft.AspNetCore.Identity;

namespace BatusSales.Api.Infrastructure;

/// <summary>
/// Until a production mail provider is integrated, only local development may
/// bypass confirmed email. Do not base this decision on a user-controlled header.
/// </summary>
public static class IdentityEmailPolicy
{
    public static void Configure(IdentityOptions options, bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.SignIn.RequireConfirmedEmail = !isDevelopment;
    }
}
