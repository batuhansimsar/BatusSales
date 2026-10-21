using BatusSales.Api.Organizations;
using Xunit;

namespace BatusSales.Tests;

public sealed class OrganizationSlugTests
{
    [Theory]
    [InlineData("My-Salon", "my-salon")]
    [InlineData("  AUTO-SERVICE-01  ", "auto-service-01")]
    [InlineData("abc", "abc")]
    public void Normalizes_valid_slugs(string input, string expected)
    {
        var accepted = OrganizationSlug.TryCreate(input, out var slug);

        Assert.True(accepted);
        Assert.NotNull(slug);
        Assert.Equal(expected, slug.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ab")]
    [InlineData("-abc")]
    [InlineData("abc-")]
    [InlineData("a--b")]
    [InlineData("My Salon")]
    [InlineData("şirket")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void Rejects_invalid_slugs(string input)
    {
        Assert.False(OrganizationSlug.TryCreate(input, out var slug));
        Assert.Null(slug);
    }

    [Fact]
    public void Rejects_null_slug()
    {
        Assert.False(OrganizationSlug.TryCreate(null, out var slug));
        Assert.Null(slug);
    }
}
