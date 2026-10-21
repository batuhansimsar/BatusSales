namespace BatusSales.Core.Tenancy;

/// <summary>A validated identifier only; NOT proof of authorization.</summary>
public sealed record TenantId
{
    public Guid Value { get; }

    private TenantId(Guid value) => Value = value;

    public static TenantId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Tenant identifier must not be empty.", nameof(value));
        }

        return new TenantId(value);
    }

    public static TenantId Parse(string? value)
    {
        if (!Guid.TryParse(value, out var parsed) || parsed == Guid.Empty)
        {
            throw new FormatException("Tenant identifier must be a non-empty GUID.");
        }

        return new TenantId(parsed);
    }

    public override string ToString() => Value.ToString("D");
}
