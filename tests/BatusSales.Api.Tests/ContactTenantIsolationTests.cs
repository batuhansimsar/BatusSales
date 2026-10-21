using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Tests;

public sealed class ContactTenantIsolationTests
{
    [Fact]
    public async Task ContactsAreInvisibleWithoutVerifiedMembership()
    {
        var options = Options();
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using (var seed = new AppDbContext(options))
        {
            await AddMembership(seed, organizationId, userId);
            Assert.True(await TenantAccess.IsMemberAsync(
                seed, User(userId), organizationId, CancellationToken.None));
            seed.Contacts.Add(new Contact { OrganizationId = organizationId, Name = "Private" });
            await seed.SaveChangesAsync();
        }

        await using var unauthenticated = new AppDbContext(options);
        Assert.Null(unauthenticated.AuthorizedTenantId);
        Assert.Empty(await unauthenticated.Contacts.ToListAsync());
    }

    [Fact]
    public async Task SameCachedEfModelScopesReadsToEachVerifiedOrganization()
    {
        var options = Options();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using (var seed = new AppDbContext(options))
        {
            await AddMembership(seed, firstId, userId);
            await AddMembership(seed, secondId, userId);
        }

        await using (var first = new AppDbContext(options))
        {
            Assert.True(await TenantAccess.IsMemberAsync(
                first, User(userId), firstId, CancellationToken.None));
            first.Contacts.Add(new Contact { OrganizationId = firstId, Name = "First" });
            await first.SaveChangesAsync();
        }

        await using (var second = new AppDbContext(options))
        {
            Assert.True(await TenantAccess.IsMemberAsync(
                second, User(userId), secondId, CancellationToken.None));
            second.Contacts.Add(new Contact { OrganizationId = secondId, Name = "Second" });
            await second.SaveChangesAsync();
        }

        await using (var firstRead = new AppDbContext(options))
        {
            Assert.True(await TenantAccess.IsMemberAsync(
                firstRead, User(userId), firstId, CancellationToken.None));
            Assert.Equal("First", (await firstRead.Contacts.SingleAsync()).Name);
        }

        await using (var secondRead = new AppDbContext(options))
        {
            Assert.True(await TenantAccess.IsMemberAsync(
                secondRead, User(userId), secondId, CancellationToken.None));
            Assert.Equal("Second", (await secondRead.Contacts.SingleAsync()).Name);
        }
    }

    [Fact]
    public async Task DeniedMembershipRevokesPreviouslyActiveTenantScope()
    {
        var options = Options();
        var firstId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var db = new AppDbContext(options);
        await AddMembership(db, firstId, userId);
        Assert.True(await TenantAccess.IsMemberAsync(
            db, User(userId), firstId, CancellationToken.None));
        Assert.Equal(firstId, db.AuthorizedTenantId);
        Assert.False(await TenantAccess.IsMemberAsync(
            db, User(userId), Guid.NewGuid(), CancellationToken.None));
        Assert.Null(db.AuthorizedTenantId);
    }

    [Fact]
    public async Task BothSyncAndAsyncSavesRejectCrossTenantContacts()
    {
        var options = Options();
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var db = new AppDbContext(options);
        await AddMembership(db, organizationId, userId);
        Assert.True(await TenantAccess.IsMemberAsync(
            db, User(userId), organizationId, CancellationToken.None));

        db.Contacts.Add(new Contact { OrganizationId = Guid.NewGuid(), Name = "Forbidden" });
        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges());
        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges(false));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync(false));
    }

    [Fact]
    public async Task MovingAnExistingContactBetweenOrganizationsIsRejected()
    {
        var options = Options();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var db = new AppDbContext(options);
        await AddMembership(db, firstId, userId);
        Assert.True(await TenantAccess.IsMemberAsync(
            db, User(userId), firstId, CancellationToken.None));
        var contact = new Contact { OrganizationId = firstId, Name = "Original" };
        db.Contacts.Add(contact);
        await db.SaveChangesAsync();

        contact.OrganizationId = secondId;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    private static DbContextOptions<AppDbContext> Options() =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    private static ClaimsPrincipal User(Guid userId) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"));

    private static async Task AddMembership(AppDbContext db, Guid organizationId, Guid userId)
    {
        db.Organizations.Add(new Organization { Id = organizationId, Name = "Demo" });
        db.Memberships.Add(new OrganizationMembership
        {
            OrganizationId = organizationId,
            UserId = userId,
            Role = OrganizationRole.Owner
        });
        await db.SaveChangesAsync();
    }
}
