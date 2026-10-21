using System.Security.Claims;
using BatusSales.Api.Auth;
using BatusSales.Api.Domain;
using Xunit;

namespace BatusSales.Api.Tests;

public sealed class TenantContextTests
{
    [Fact]
    public void Unauthenticated_claims_are_not_accepted()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("tenant_id", Guid.NewGuid().ToString())]));
        Assert.Null(TenantContext.ReadTenantId(user));
    }

    [Fact]
    public void Valid_authenticated_tenant_claim_is_accepted()
    {
        var tenantId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("tenant_id", tenantId.ToString())], "test"));
        Assert.Equal(tenantId, TenantContext.ReadTenantId(user));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Invalid_tenant_claims_are_rejected(string claim)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("tenant_id", claim)], "test"));
        Assert.Null(TenantContext.ReadTenantId(user));
    }

    [Fact]
    public void Contact_rejects_missing_tenant()
    {
        Assert.Throws<ArgumentException>(() => new Contact(Guid.Empty, "Sample", null));
    }

    [Fact]
    public void Contact_normalizes_input_and_keeps_tenant()
    {
        var tenantId = Guid.NewGuid();
        var contact = new Contact(tenantId, "  Sample  ", "  sample@example.com  ");
        Assert.Equal(tenantId, contact.TenantId);
        Assert.Equal("Sample", contact.Name);
        Assert.Equal("sample@example.com", contact.Email);
        Assert.NotEqual(Guid.Empty, contact.Id);
    }
}
