using Microsoft.AspNetCore.Identity;

namespace BatusSales.Api.Domain;

public sealed class ApplicationUser : IdentityUser
{
}

public sealed class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class Membership
{
    public Guid OrganizationId { get; set; }
    public required string UserId { get; set; }
    public required string Role { get; set; }
}

public sealed class Contact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CurrentTenant
{
    public Guid? Id { get; set; }
}
