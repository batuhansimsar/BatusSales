using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BatusSales.Api.Tests;

/// <summary>
/// Runs against a disposable PostgreSQL service in GitHub Actions, not EF InMemory/SQLite.
/// The environment variable is intentionally absent for the default local unit-test run.
/// Never point BATUSSALES_TEST_POSTGRES at a customer or production database.
/// </summary>
public sealed class PostgresTenantIsolationTests
{
    [Fact]
    public async Task PostgreSql_filters_contacts_by_tenant_and_rejects_cross_tenant_writes()
    {
        var connectionString = Environment.GetEnvironmentVariable("BATUSSALES_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // CI supplies this variable and verifies it is nonempty before dotnet test.
            return;
        }

        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var firstTenant = new CurrentTenant { Id = firstId };
        var secondTenant = new CurrentTenant { Id = secondId };

        // This is an isolated CI database. EnsureCreated is test setup ONLY, not a
        // recommended application migration or deployment strategy.
        await using (var setup = new SalesDbContext(options, firstTenant))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Organizations.AddRange(
                new Organization { Id = firstId, Name = "Tenant one" },
                new Organization { Id = secondId, Name = "Tenant two" });
            setup.Contacts.Add(new Contact { OrganizationId = firstId, Name = "First contact" });
            await setup.SaveChangesAsync();
        }

        await using (var setup = new SalesDbContext(options, secondTenant))
        {
            setup.Contacts.Add(new Contact { OrganizationId = secondId, Name = "Second contact" });
            await setup.SaveChangesAsync();
        }

        // Sharing EF's compiled model must not freeze the first context's tenant.
        await using (var first = new SalesDbContext(options, firstTenant))
        {
            var visible = await first.Contacts.AsNoTracking().ToListAsync();
            Assert.Single(visible);
            Assert.Equal(firstId, visible[0].OrganizationId);
        }

        await using (var second = new SalesDbContext(options, secondTenant))
        {
            var visible = await second.Contacts.AsNoTracking().ToListAsync();
            Assert.Single(visible);
            Assert.Equal(secondId, visible[0].OrganizationId);
        }

        await using (var anonymous = new SalesDbContext(options, new CurrentTenant()))
        {
            Assert.Empty(await anonymous.Contacts.AsNoTracking().ToListAsync());
            anonymous.Contacts.Add(new Contact
            {
                OrganizationId = firstId,
                Name = "Unauthorized write"
            });
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => anonymous.SaveChangesAsync());
        }

        await using (var crossTenant = new SalesDbContext(options, firstTenant))
        {
            crossTenant.Contacts.Add(new Contact
            {
                OrganizationId = secondId,
                Name = "Cross-tenant write"
            });
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => crossTenant.SaveChangesAsync());
        }
    }
}
