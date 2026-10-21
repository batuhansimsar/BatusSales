namespace BatusSales.Api.Tenancy;

/// <summary>A typed organization boundary identifier, not a proof of authorization.</summary>
public readonly record struct TenantId(Guid Value)
{
    public static bool TryParse(string? input, out TenantId tenantId)
    {
        tenantId = default;

        if (!Guid.TryParseExact(input, "D", out var id) || id == Guid.Empty)
        {
            return false;
        }

        tenantId = new TenantId(id);
        return true;
    }

    public override string ToString() => Value.ToString("D");
}
