using BatusSales.Api.Tenancy;
using Xunit;

namespace BatusSales.Api.Tests;

public sealed class TenantContextTests
{
    [Fact]
    public void Tenant_is_unbound_by_default()
    {
        var context = new TenantContext();
        Assert.False(context.HasTenant);
        Assert.Equal(Guid.Empty, context.OrganizationId);
    }

    [Fact]
    public void Tenant_cannot_be_changed_mid_request()
    {
        var context = new TenantContext();
        var a = Guid.NewGuid();
        context.InitializeAfterAuthorization(a);
        context.InitializeAfterAuthorization(a);
        Assert.Throws<InvalidOperationException>(() =>
            context.InitializeAfterAuthorization(Guid.NewGuid()));
    }

    [Fact]
    public void Empty_tenant_cannot_be_bound()
    {
        var context = new TenantContext();
        Assert.Throws<ArgumentException>(() =>
            context.InitializeAfterAuthorization(Guid.Empty));
    }
}
