namespace BatusSales.Core.Tenancy;

/// <summary>
/// Defense-in-depth for domain operations. Caller must first resolve the
/// authenticated user's membership; never trust an arbitrary tenant header.
/// </summary>
public static class TenantAccess
{
    public static void EnsureSameTenant(TenantId currentTenant, TenantId resourceTenant)
    {
        ArgumentNullException.ThrowIfNull(currentTenant);
        ArgumentNullException.ThrowIfNull(resourceTenant);

        if (currentTenant != resourceTenant)
        {
            // Avoid disclosing whether another tenant's resource exists.
            throw new UnauthorizedAccessException("Resource not available.");
        }
    }
}
