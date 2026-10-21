using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BatusSales.Api.Tests;

public sealed class TenantIsolationTests
{
    [Fact]
    public async Task Contacts_from_other_organizations_are_not_visible()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite(connection).Options;

        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var first = new CurrentTenant { Id = firstId };
        var second = new CurrentTenant { Id = secondId };

        await using (var setup = new SalesDbContext(options, first))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Organizations.AddRange(
                new Organization { Id = firstId, Name = "First" },
                new Organization { Id = secondId, Name = "Second" });
            setup.Contacts.Add(new Contact
            {
                OrganizationId = firstId,
                Name = "First Contact"
            });
            await setup.SaveChangesAsync();
        }

        await using (var other = new SalesDbContext(options, second))
        {
            Assert.Empty(await other.Contacts.ToListAsync());
        }

        await using (var own = new SalesDbContext(options, first))
        {
            Assert.Single(await own.Contacts.ToListAsync());
        }

        await using (var noTenant = new SalesDbContext(options, new CurrentTenant()))
        {
            Assert.Empty(await noTenant.Contacts.ToListAsync());
        }
    }

    [Fact]
    public async Task Cross_tenant_contact_writes_are_rejected()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite(connection).Options;
        var tenant = new CurrentTenant { Id = Guid.NewGuid() };
        await using var db = new SalesDbContext(options, tenant);
        db.Contacts.Add(new Contact
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Forbidden"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => db.SaveChangesAsync());
    }

    [Fact]
    public void Synchronous_save_rejects_cross_tenant_contact_writes()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite("DataSource=:memory:").Options;
        using var db = new SalesDbContext(options, new CurrentTenant { Id = Guid.NewGuid() });
        db.Contacts.Add(new Contact
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Not owned"
        });

        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges());
        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges(false));
    }

    [Fact]
    public async Task Async_save_with_explicit_accept_flag_rejects_cross_tenant_contact_writes()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite("DataSource=:memory:").Options;
        await using var db = new SalesDbContext(options, new CurrentTenant { Id = Guid.NewGuid() });
        db.Contacts.Add(new Contact
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Not owned"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync(false));
    }

    [Fact]
    public async Task Async_save_without_authorized_tenant_fails_closed()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseSqlite("DataSource=:memory:").Options;
        await using var db = new SalesDbContext(options, new CurrentTenant());
        db.Contacts.Add(new Contact
        {
            OrganizationId = Guid.NewGuid(),
            Name = "No membership"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

}
