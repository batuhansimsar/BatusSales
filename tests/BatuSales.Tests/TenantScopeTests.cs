using Xunit;
using BatuSales.Api.Security;

namespace BatuSales.Tests;

public sealed class TenantScopeTests
{
    [Fact]
    public void NewScopeHasNoOrganization()
    {
        Assert.Null(new TenantScope().OrganizationId);
    }

    [Fact]
    public void ScopeCannotSwitchOrganizations()
    {
        var scope = new TenantScope();
        var first = Guid.NewGuid();
        scope.Enter(first);
        scope.Enter(first);
        Assert.Equal(first, scope.OrganizationId);
        Assert.Throws<InvalidOperationException>(() => scope.Enter(Guid.NewGuid()));
        Assert.Equal(first, scope.OrganizationId);
    }

    [Fact]
    public void EmptyIdIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new TenantScope().Enter(Guid.Empty));
    }
}
