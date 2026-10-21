using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Tests;

public sealed class TenantContactGuardTests
{
    private static ClaimsPrincipal Principal(Guid userId) => new(
        new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"));

    [Fact]
    public async Task Contacts_are_invisible_without_verified_membership_and_after_denial()
    {
        await using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var organizationA = Guid.NewGuid();
        var organizationB = Guid.NewGuid();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        await using (var setup = new AppDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Users.AddRange(
                new AppUser { Id = userA, UserName = "user-a", Email = "a@example.com" },
                new AppUser { Id = userB, UserName = "user-b", Email = "b@example.com" });
            setup.Organizations.AddRange(
                new Organization { Id = organizationA, Name = "A" },
                new Organization { Id = organizationB, Name = "B" });
            setup.Memberships.AddRange(
                new OrganizationMembership
                {
                    OrganizationId = organizationA, UserId = userA, Role = OrganizationRole.Owner
                },
                new OrganizationMembership
                {
                    OrganizationId = organizationB, UserId = userB, Role = OrganizationRole.Owner
                });
            await setup.SaveChangesAsync();

            Assert.True(await TenantAccess.IsMemberAsync(
                setup, Principal(userB), organizationB, CancellationToken.None));
            setup.Contacts.Add(new Contact { OrganizationId = organizationB, Name = "B only" });
            await setup.SaveChangesAsync();
        }

        await using var request = new AppDbContext(options);
        Assert.Empty(await request.Contacts.ToListAsync());
        Assert.True(await TenantAccess.IsMemberAsync(
            request, Principal(userA), organizationA, CancellationToken.None));
        Assert.Empty(await request.Contacts.ToListAsync());
        Assert.False(await TenantAccess.IsMemberAsync(
            request, Principal(userA), organizationB, CancellationToken.None));
        Assert.Null(request.ActiveTenantId);
        Assert.Empty(await request.Contacts.ToListAsync());
        Assert.True(await TenantAccess.IsMemberAsync(
            request, Principal(userB), organizationB, CancellationToken.None));
        Assert.Single(await request.Contacts.ToListAsync());
    }

    [Fact]
    public async Task All_save_paths_reject_unverified_cross_tenant_contact_writes()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:").Options;
        await using var db = new AppDbContext(options);
        db.Contacts.Add(new Contact { OrganizationId = Guid.NewGuid(), Name = "Forbidden" });

        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges());
        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges(false));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync(false));
    }

    [Fact]
    public async Task Guessed_foreign_contact_id_does_not_allow_detached_update()
    {
        await using var conn = new SqliteConnection("DataSource=:memory:");
        await conn.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        var organizationA = Guid.NewGuid();
        var organizationB = Guid.NewGuid();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        await using (var setup = new AppDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Users.AddRange(
                new AppUser { Id = userA, UserName = "user-a", Email = "a@example.com" },
                new AppUser { Id = userB, UserName = "user-b", Email = "b@example.com" });
            setup.Organizations.AddRange(
                new Organization { Id = organizationA, Name = "A" },
                new Organization { Id = organizationB, Name = "B" });
            setup.Memberships.AddRange(
                new OrganizationMembership
                {
                    OrganizationId = organizationA, UserId = userA, Role = OrganizationRole.Owner
                },
                new OrganizationMembership
                {
                    OrganizationId = organizationB, UserId = userB, Role = OrganizationRole.Owner
                });
            await setup.SaveChangesAsync();
            Assert.True(await TenantAccess.IsMemberAsync(
                setup, Principal(userB), organizationB, CancellationToken.None));
            setup.Contacts.Add(new Contact
            {
                Id = contactId, OrganizationId = organizationB, Name = "Private B"
            });
            await setup.SaveChangesAsync();
        }

        await using (var attacker = new AppDbContext(options))
        {
            Assert.True(await TenantAccess.IsMemberAsync(
                attacker, Principal(userA), organizationA, CancellationToken.None));

            // A fake detached object claims a different tenant's row is owned by A.
            var forged = new Contact
            {
                Id = contactId, OrganizationId = organizationA, Name = "Changed by A"
            };
            attacker.Contacts.Attach(forged);
            attacker.Entry(forged).Property(c => c.Name).IsModified = true;

            // OrganizationId is a concurrency token, so the database checks both
            // the guessed ID and the original organization ID in the UPDATE.
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => attacker.SaveChangesAsync());
        }

        await using (var verify = new AppDbContext(options))
        {
            var original = await verify.Contacts.IgnoreQueryFilters().SingleAsync(c => c.Id == contactId);
            Assert.Equal("Private B", original.Name);
            Assert.Equal(organizationB, original.OrganizationId);
        }
    }
}
