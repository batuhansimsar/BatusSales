using Microsoft.AspNetCore.Identity;

namespace BatusSales.Api.Data;

public sealed class AppUser : IdentityUser;

public sealed class Organization
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public required string CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public static class OrganizationRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Agent = "Agent";
}

public sealed class OrganizationMembership
{
    public Guid OrganizationId { get; set; }
    public required string UserId { get; set; }
    public required string Role { get; set; }
}

public sealed class Contact
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid OrganizationId { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AuditEvent
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid OrganizationId { get; set; }
    public required string ActorUserId { get; set; }
    public required string Action { get; set; }
    public Guid EntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
