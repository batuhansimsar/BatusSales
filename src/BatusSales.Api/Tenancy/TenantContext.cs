namespace BatusSales.Api.Tenancy;

/// <summary>
/// Scoped, fail-closed tenant state. No public API route currently initializes
/// it. Future auth middleware must verify a membership before initialization.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private Guid _organizationId;

    public Guid OrganizationId => _organizationId;
    public bool HasTenant => _organizationId != Guid.Empty;

    public void InitializeAfterAuthorization(Guid organizationId)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization ID cannot be empty.", nameof(organizationId));

        if (HasTenant && _organizationId != organizationId)
            throw new InvalidOperationException("Tenant context cannot be switched within a request.");

        _organizationId = organizationId;
    }
}
