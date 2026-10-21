using BatusSales.Core.Tenancy;
using Xunit;

namespace BatusSales.Tests;

public class TenancyTests
{
    [Fact]
    public void EmptyTenantIsRejected()
    {
        Assert.Throws<ArgumentException>(() => TenantId.From(Guid.Empty));
        Assert.Throws<FormatException>(() => TenantId.Parse(Guid.Empty.ToString()));
        Assert.Throws<FormatException>(() => TenantId.Parse("not-a-guid"));
        Assert.Throws<FormatException>(() => TenantId.Parse(null));
    }

    [Fact]
    public void SameTenantIsAccepted()
    {
        var id = Guid.NewGuid();
        TenantAccess.EnsureSameTenant(TenantId.From(id), TenantId.Parse(id.ToString()));
    }

    [Fact]
    public void CrossTenantAccessIsDenied()
    {
        var current = TenantId.From(Guid.NewGuid());
        var other = TenantId.From(Guid.NewGuid());

        Assert.Throws<UnauthorizedAccessException>(
            () => TenantAccess.EnsureSameTenant(current, other));
    }
}
