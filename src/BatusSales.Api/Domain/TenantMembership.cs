namespace BatusSales.Api.Domain;

// Memberships are currently provisioned through trusted administrative SQL only.
public sealed class TenantMembership
{
    public Guid TenantId { get; set; }
    public string SubjectId { get; set; } = string.Empty;
}
