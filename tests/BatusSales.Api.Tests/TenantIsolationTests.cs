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
}
