using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Xunit;

namespace BatusSales.Api.IntegrationTests;

// A real PostgreSQL database is mandatory. No in-memory fallback and no production DB.
public sealed class PostgreSqlApiFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable("BATUSSALES_TEST_POSTGRES")
            ?? throw new InvalidOperationException(
                "Set BATUSSALES_TEST_POSTGRES to a disposable PostgreSQL test database.");

        var parsed = new NpgsqlConnectionStringBuilder(connectionString);
        if (!string.Equals(parsed.Database, "batussales_test", StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Refusing integration tests against a non-test database.");

        // Host configuration is available before the API's top-level CreateBuilder() runs.
        builder.ConfigureHostConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connectionString
            }));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseEnvironment("Development");
}

public sealed class TenantIsolationHttpTests(PostgreSqlApiFactory factory)
    : IClassFixture<PostgreSqlApiFactory>
{
    [Fact]
    public async Task MembershipEnforcedForEveryContactMethodWithRealPostgres()
    {
        using var owner = CreateClient();
        using var outsider = CreateClient();

        var anonymous = await owner.GetAsync(
            $"/api/organizations/{Guid.NewGuid()}/contacts");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        await AuthenticateAsync(owner);
        await AuthenticateAsync(outsider);

        var ownerOrg = await CreateOrganizationAsync(owner, "Owner organization");
        var outsiderOrg = await CreateOrganizationAsync(outsider, "Outsider organization");
        var ownerContact = await CreateContactAsync(owner, ownerOrg);

        var ownerList = await owner.GetAsync($"/api/organizations/{ownerOrg}/contacts");
        Assert.Equal(HttpStatusCode.OK, ownerList.StatusCode);
        var listJson = await ownerList.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(listJson.GetProperty("items").EnumerateArray(),
            contact => contact.GetProperty("id").GetGuid() == ownerContact);

        Assert.Equal(HttpStatusCode.NotFound,
            (await outsider.GetAsync($"/api/organizations/{ownerOrg}/contacts")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await outsider.GetAsync(
                $"/api/organizations/{ownerOrg}/contacts/{ownerContact}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await outsider.PostAsJsonAsync(
                $"/api/organizations/{ownerOrg}/contacts",
                new { name = "Unauthorized", email = "wrong@example.test", phone = (string?)null }))
                .StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await outsider.PutAsJsonAsync(
                $"/api/organizations/{ownerOrg}/contacts/{ownerContact}",
                new { name = "Tampered", email = "wrong@example.test", phone = (string?)null }))
                .StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await outsider.DeleteAsync(
                $"/api/organizations/{ownerOrg}/contacts/{ownerContact}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await owner.GetAsync($"/api/organizations/{outsiderOrg}/contacts")).StatusCode);

        var stillThere = await owner.GetFromJsonAsync<JsonElement>(
            $"/api/organizations/{ownerOrg}/contacts/{ownerContact}");
        Assert.Equal("Original", stillThere.GetProperty("name").GetString());

        // Also exercise the soft-delete query filter on a real relational provider.
        Assert.Equal(HttpStatusCode.NoContent,
            (await owner.DeleteAsync(
                $"/api/organizations/{ownerOrg}/contacts/{ownerContact}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await owner.GetAsync(
                $"/api/organizations/{ownerOrg}/contacts/{ownerContact}")).StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var deleted = await db.Contacts.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(x => x.Id == ownerContact);
        Assert.True(deleted.IsDeleted);
        Assert.Equal("Original", deleted.Name);
        Assert.Equal(0, await db.AuditEvents.CountAsync(x =>
            x.OrganizationId == ownerOrg && x.Action == "contact.updated"));
    }

    [Fact]
    public async Task RevokedMembershipIsDeniedWithPreviouslyIssuedToken()
    {
        using var owner = CreateClient();
        await AuthenticateAsync(owner);

        var organizationId = await CreateOrganizationAsync(owner, "Revocation test organization");
        var contactId = await CreateContactAsync(owner, organizationId);
        var contactsPath = $"/api/organizations/{organizationId}/contacts";

        // The bearer token remains valid, but the membership is removed in the
        // authoritative PostgreSQL database. Authorization must fail immediately,
        // not only after the access token expires or is refreshed.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var membership = await db.Memberships.SingleAsync(
                x => x.OrganizationId == organizationId);
            db.Memberships.Remove(membership);
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.NotFound,
            (await owner.GetAsync(contactsPath)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await owner.GetAsync($"{contactsPath}/{contactId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await owner.PostAsJsonAsync(contactsPath,
                new { name = "Forbidden", email = "forbidden@example.test", phone = (string?)null }))
            .StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await owner.PutAsJsonAsync($"{contactsPath}/{contactId}",
                new { name = "Forbidden", email = "forbidden@example.test", phone = (string?)null }))
            .StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await owner.DeleteAsync($"{contactsPath}/{contactId}")).StatusCode);

        // Revocation must also remove this organization from the same token's listing.
        var listedOrganizations = await owner.GetFromJsonAsync<JsonElement>("/api/organizations");
        Assert.False(listedOrganizations.EnumerateArray().Any(
            x => x.GetProperty("id").GetGuid() == organizationId));

        // The revoked user's token must not cause a write or delete in this tenant.
        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Independent test-only database inspection bypasses future tenant query filters;
        // never use IgnoreQueryFilters in an HTTP business endpoint.
        var contact = await verifyDb.Contacts.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(x => x.OrganizationId == organizationId && x.Id == contactId);
        Assert.Equal("Original", contact.Name);
        Assert.False(contact.IsDeleted);
        Assert.Equal(1, await verifyDb.AuditEvents.CountAsync(x =>
            x.OrganizationId == organizationId && x.Action == "contact.created"));
        Assert.Equal(0, await verifyDb.AuditEvents.CountAsync(x =>
            x.OrganizationId == organizationId &&
            (x.Action == "contact.updated" || x.Action == "contact.deleted")));
    }

    [Fact]
    public async Task AuditHistoryIsScopedToTenantAndManagerRole()
    {
        using var owner = CreateClient();
        using var other = CreateClient();
        await AuthenticateAsync(owner);
        await AuthenticateAsync(other);

        var ownerOrg = await CreateOrganizationAsync(owner, "Audit organization");
        var otherOrg = await CreateOrganizationAsync(other, "Separate organization");
        await CreateContactAsync(owner, ownerOrg);
        var auditPath = $"/api/organizations/{ownerOrg}/audit-events";

        var anonymousClient = CreateClient();
        using (anonymousClient)
        {
            Assert.Equal(HttpStatusCode.Unauthorized,
                (await anonymousClient.GetAsync(auditPath)).StatusCode);
        }

        // Owner can inspect only this organization's existing audit events.
        var firstPage = await owner.GetAsync(auditPath + "?page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, firstPage.StatusCode);
        var pageJson = await firstPage.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, pageJson.GetProperty("total").GetInt32());
        Assert.Single(pageJson.GetProperty("items").EnumerateArray());
        var all = await owner.GetFromJsonAsync<JsonElement>(auditPath + "?pageSize=100");
        var rows = all.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, entry =>
            Assert.Equal(ownerOrg, entry.GetProperty("organizationId").GetGuid()));
        Assert.Contains(rows, entry =>
            entry.GetProperty("action").GetString() == "organization.created");
        Assert.Contains(rows, entry =>
            entry.GetProperty("action").GetString() == "contact.created");

        // Non-members cannot even infer whether another tenant has audit events.
        Assert.Equal(HttpStatusCode.NotFound,
            (await other.GetAsync(auditPath)).StatusCode);

        // A staff member assigned the Agent role may use CRM but cannot
        // inspect privileged audit history for that organization.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var otherUserId = await db.Memberships
                .Where(x => x.OrganizationId == otherOrg)
                .Select(x => x.UserId).SingleAsync();
            db.Memberships.Add(new OrganizationMembership
            {
                OrganizationId = ownerOrg,
                UserId = otherUserId,
                Role = OrganizationRole.Agent
            });
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Forbidden,
            (await other.GetAsync(auditPath)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await other.GetAsync($"/api/organizations/{otherOrg}/audit-events")).StatusCode);
    }

    private HttpClient CreateClient() =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

    private static async Task AuthenticateAsync(HttpClient client)
    {
        var email = $"tenant-{Guid.NewGuid():N}@example.test";
        const string password = "Strong-Integration-Password_2026!";

        var register = await client.PostAsJsonAsync(
            "/auth/register", new { email, password });
        Assert.True(register.IsSuccessStatusCode,
            $"Registration failed: {(int)register.StatusCode} {await register.Content.ReadAsStringAsync()}");

        var login = await client.PostAsJsonAsync(
            "/auth/login?useCookies=false", new { email, password });
        Assert.True(login.IsSuccessStatusCode,
            $"Login failed: {(int)login.StatusCode} {await login.Content.ReadAsStringAsync()}");

        var json = await login.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", json.GetProperty("accessToken").GetString());
    }

    private static async Task<Guid> CreateOrganizationAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync(
            "/api/organizations", new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateContactAsync(HttpClient client, Guid organizationId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/organizations/{organizationId}/contacts",
            new { name = "Original", email = "original@example.test", phone = (string?)null });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();
    }
}
