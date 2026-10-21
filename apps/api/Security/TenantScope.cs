namespace BatuSales.Api.Security;

/// <summary>
/// A request-scoped, membership-verified tenant identity. Never populate this
/// directly from a user-controlled header or an unverified token claim.
/// </summary>
public sealed class TenantScope
{
    public Guid? OrganizationId { get; private set; }

    public void Enter(Guid organizationId)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization ID cannot be empty.", nameof(organizationId));

        if (OrganizationId.HasValue && OrganizationId.Value != organizationId)
            throw new InvalidOperationException("A request cannot switch organizations.");

        OrganizationId = organizationId;
    }
}
