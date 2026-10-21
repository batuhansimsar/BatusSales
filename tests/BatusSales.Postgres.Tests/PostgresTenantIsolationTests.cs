using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BatusSales.Postgres.Tests;

// Real-provider tests, intentionally kept in a separate project so a missing database
// cannot cause a silent test skip or break the fast SQLite/unit-test suite.
public sealed class PostgresTenantIsolationTests : IAsyncLifetime
{
    private readonly string _connectionString =
        Environment.GetEnvironmentVariable("BATUSSALES_TEST_POSTGRES")
        ?? throw new InvalidOperationException(
            "BATUSSALES_TEST_POSTGRES is required for PostgreSQL integration tests.");

    private SalesDbContext OpenDb(Guid? organizationId = null)
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new SalesDbContext(options, new CurrentTenant { Id = organizationId });
    }

    public async Task InitializeAsync()
    {
        // Ephemeral CI database only. Do not use EnsureCreated in production.
        await using var db = OpenDb();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PostgreSql_filters_other_organizations_by_tenant_even_for_known_contact_id()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var foreignContact = new Contact { OrganizationId = secondId, Name = "Foreign" };
        await using (var setup = OpenDb())
        {
            setup.Organizations.AddRange(
                new Organization { Id = firstId, Name = "First" },
                new Organization { Id = secondId, Name = "Second" });
            await setup.SaveChangesAsync();
        }

        await using (var creator = OpenDb(secondId))
        {
            creator.Contacts.Add(foreignContact);
            await creator.SaveChangesAsync();
        }

        await using (var first = OpenDb(firstId))
        {
            Assert.Null(await first.Contacts.SingleOrDefaultAsync(x => x.Id == foreignContact.Id));
            Assert.Empty(await first.Contacts.ToListAsync());
        }

        await using (var second = OpenDb(secondId))
        {
            Assert.NotNull(await second.Contacts.SingleOrDefaultAsync(x => x.Id == foreignContact.Id));
        }

        await using (var noTenant = OpenDb())
        {
            Assert.Null(await noTenant.Contacts.SingleOrDefaultAsync(x => x.Id == foreignContact.Id));
        }
    }

    [Fact]
    public async Task PostgreSql_rejects_cross_tenant_mutation_before_persisting()
    {
        var activeId = Guid.NewGuid();
        await using var db = OpenDb(activeId);
        Assert.True(await db.Database.CanConnectAsync());
        db.Contacts.Add(new Contact { OrganizationId = Guid.NewGuid(), Name = "Forbidden" });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges());
    }

    [Fact]
    public async Task Membership_filter_denies_a_different_organization_on_postgresql()
    {
        var ownedOrganizationId = Guid.NewGuid();
        var foreignOrganizationId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            UserName = $"member-{Guid.NewGuid():N}@example.test",
            Email = $"member-{Guid.NewGuid():N}@example.test"
        };

        await using var db = OpenDb();
        db.Users.Add(user);
        db.Organizations.AddRange(
            new Organization { Id = ownedOrganizationId, Name = "Owned" },
            new Organization { Id = foreignOrganizationId, Name = "Foreign" });
        db.Memberships.Add(new Membership
        {
            OrganizationId = ownedOrganizationId, UserId = user.Id, Role = "Owner"
        });
        await db.SaveChangesAsync();

        var http = new DefaultHttpContext();
        http.Request.RouteValues["organizationId"] = foreignOrganizationId.ToString();
        http.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, user.Id)], "test"));
        var currentTenant = new CurrentTenant();
        using var services = new ServiceCollection()
            .AddSingleton(db)
            .AddSingleton(currentTenant)
            .BuildServiceProvider();
        http.RequestServices = services;

        var nextInvoked = false;
        var result = await new TenantAccessFilter().InvokeAsync(
            EndpointFilterInvocationContext.Create(http),
            _ =>
            {
                nextInvoked = true;
                return ValueTask.FromResult<object?>(Results.Ok());
            });

        Assert.False(nextInvoked);
        Assert.Equal(StatusCodes.Status404NotFound,
            Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Null(currentTenant.Id);
    }
}
