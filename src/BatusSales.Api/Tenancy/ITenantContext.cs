namespace BatusSales.Api.Tenancy;

/// <summary>
/// A scoped organization context that may ONLY be initialized after a trusted
/// authentication and organization-membership authorization step.
/// Never populate it directly from an unverified HTTP header or agent output.
/// </summary>
public interface ITenantContext
{
    Guid OrganizationId { get; }
    bool HasTenant { get; }
}
