using BatusSales.Api.Tenancy;
using Xunit;

namespace BatusSales.Tests;

public sealed class TenantIdTests
{
    [Fact]
    public void Accepts_non_empty_canonical_guid()
    {
        const string input = "bb8c59cf-bd04-4b98-8ff1-a926cdfb1949";

        Assert.True(TenantId.TryParse(input, out var tenantId));
        Assert.Equal(input, tenantId.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("bb8c59cfbd044b988ff1a926cdfb1949")]
    public void Rejects_missing_or_invalid_tenant_ids(string input)
    {
        Assert.False(TenantId.TryParse(input, out _));
    }
}
