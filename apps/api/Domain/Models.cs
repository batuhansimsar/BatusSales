namespace BatuSales.Api.Domain;

public sealed class Organization
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public static class OrganizationRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Agent = "Agent";
}

public sealed class OrganizationMember
{
    public Guid OrganizationId { get; set; }
    public required string UserId { get; set; }
    public required string Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Contact
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed record CreateOrganizationRequest(string Name);
public sealed record CreateContactRequest(string FullName, string? Email, string? Phone, string? Notes);
