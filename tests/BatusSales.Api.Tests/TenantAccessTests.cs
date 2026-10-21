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

    [Theory]
    [InlineData(OrganizationRole.Owner, true)]
    [InlineData(OrganizationRole.Admin, true)]
    [InlineData(OrganizationRole.Agent, false)]
    public void OnlyPrivilegedMembershipsCanDeleteContacts(OrganizationRole role, bool expected)
    {
        Assert.Equal(expected, TenantAccess.CanDeleteContacts(role));
    }

    [Fact]
    public async Task DeleteRoleLookupIsScopedToAuthenticatedUserAndOrganization()
    {
        var userId = Guid.NewGuid();
        var anotherUser = Guid.NewGuid();
        var organizationA = Guid.NewGuid();
        var organizationB = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        await using var db = new AppDbContext(options);
        db.Memberships.AddRange(
            new OrganizationMembership
            {
                OrganizationId = organizationA, UserId = userId, Role = OrganizationRole.Agent
            },
            new OrganizationMembership
            {
                OrganizationId = organizationB, UserId = anotherUser, Role = OrganizationRole.Owner
            });
        await db.SaveChangesAsync();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"));

        Assert.Equal(OrganizationRole.Agent,
            await TenantAccess.GetRoleAsync(db, user, organizationA, CancellationToken.None));
        Assert.Null(await TenantAccess.GetRoleAsync(db, user, organizationB, CancellationToken.None));
        Assert.Null(await TenantAccess.GetRoleAsync(
            db, new ClaimsPrincipal(), organizationA, CancellationToken.None));
        Assert.Null(await TenantAccess.GetRoleAsync(db, user, Guid.Empty, CancellationToken.None));
    }
}
