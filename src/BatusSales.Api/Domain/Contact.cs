namespace BatusSales.Api.Domain;

public sealed class Contact
{
    private Contact() { } // EF Core

    public Contact(Guid tenantId, string name, string? email)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("A tenant is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A name is required.", nameof(name));

        TenantId = tenantId;
        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
