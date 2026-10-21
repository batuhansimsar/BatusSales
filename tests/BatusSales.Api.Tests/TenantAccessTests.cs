using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Tests;

public sealed class TenantAccessTests
{
    [Fact]
    public async Task MembershipIsRequiredForExactOrganization()
    {
        var ownerId = Guid.NewGuid();
        var organizationA = Guid.NewGuid();
        var organizationB = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        await using var db = new AppDbContext(options);
        db.Memberships.Add(new OrganizationMembership
        {
            OrganizationId = organizationA, UserId = ownerId, Role = OrganizationRole.Owner
        });
        await db.SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, ownerId.ToString())], "test"));

        Assert.True(await TenantAccess.IsMemberAsync(db, user, organizationA, CancellationToken.None));
        Assert.False(await TenantAccess.IsMemberAsync(db, user, organizationB, CancellationToken.None));
    }

    [Fact]
    public async Task MissingIdentityCannotAccessOrganization()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new AppDbContext(options);

        Assert.False(await TenantAccess.IsMemberAsync(
            db, new ClaimsPrincipal(), Guid.NewGuid(), CancellationToken.None));
    }
}
