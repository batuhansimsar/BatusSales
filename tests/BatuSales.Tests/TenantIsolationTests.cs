using BatuSales.Api.Data;
using BatuSales.Api.Domain;
using BatuSales.Api.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BatuSales.Tests;

/// <summary>
/// Fast isolation checks. Real PostgreSQL/API integration tests are still required.
/// </summary>
public sealed class TenantIsolationTests
{
    [Fact]
    public async Task TenantQueriesCannotReadAnotherOrganizationsContacts()
    {
        var options = new DbContextOptionsBuilder<BatuSalesDbContext>()
            .UseInMemoryDatabase($"isolation-{Guid.NewGuid()}").Options;
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var firstScope = new TenantScope();
        firstScope.Enter(first);

        await using (var writer = new BatuSalesDbContext(options, firstScope))
        {
            writer.Contacts.Add(new Contact
            {
                Id = Guid.NewGuid(), OrganizationId = first,
                FullName = "Tenant A Contact", CreatedAt = DateTimeOffset.UtcNow
            });
            await writer.SaveChangesAsync();
        }

        var secondScope = new TenantScope();
        secondScope.Enter(second);
        await using (var reader = new BatuSalesDbContext(options, secondScope))
            Assert.Empty(await reader.Contacts.ToListAsync());

        await using (var firstReader = new BatuSalesDbContext(options, firstScope))
            Assert.Single(await firstReader.Contacts.ToListAsync());

        await using (var noScopeReader = new BatuSalesDbContext(options, new TenantScope()))
            Assert.Empty(await noScopeReader.Contacts.ToListAsync());
    }

    [Fact]
    public async Task CrossTenantInsertIsRejected()
    {
        var options = new DbContextOptionsBuilder<BatuSalesDbContext>()
            .UseInMemoryDatabase($"write-guard-{Guid.NewGuid()}").Options;
        var scope = new TenantScope();
        scope.Enter(Guid.NewGuid());
        await using var db = new BatuSalesDbContext(options, scope);

        db.Contacts.Add(new Contact
        {
            Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(),
            FullName = "Should fail", CreatedAt = DateTimeOffset.UtcNow
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task MissingTenantScopeRejectsWrites()
    {
        var options = new DbContextOptionsBuilder<BatuSalesDbContext>()
            .UseInMemoryDatabase($"missing-scope-{Guid.NewGuid()}").Options;
        await using var db = new BatuSalesDbContext(options, new TenantScope());

        db.Contacts.Add(new Contact
        {
            Id = Guid.NewGuid(), OrganizationId = Guid.NewGuid(),
            FullName = "Should fail", CreatedAt = DateTimeOffset.UtcNow
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
}
