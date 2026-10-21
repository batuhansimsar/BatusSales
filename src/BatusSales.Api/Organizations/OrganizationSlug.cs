namespace BatusSales.Api.Organizations;

/// <summary>Canonical, URL-safe organization identifier. Unique constraint comes with persistence.</summary>
public sealed record OrganizationSlug
{
    private OrganizationSlug(string value) => Value = value;

    public string Value { get; }

    public static bool TryCreate(string? input, out OrganizationSlug? slug)
    {
        slug = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var canonical = input.Trim().ToLowerInvariant();

        if (canonical.Length is < 3 or > 40 || canonical[0] == '-' || canonical[^1] == '-')
        {
            return false;
        }

        var previousWasHyphen = false;
        foreach (var character in canonical)
        {
            var allowed = character is >= 'a' and <= 'z'
                or >= '0' and <= '9' or '-';

            if (!allowed || (character == '-' && previousWasHyphen))
            {
                return false;
            }

            previousWasHyphen = character == '-';
        }

        slug = new OrganizationSlug(canonical);
        return true;
    }

    public override string ToString() => Value;
}
