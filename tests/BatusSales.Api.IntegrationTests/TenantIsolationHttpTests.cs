using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BatusSales.Api.Data;
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
        Assert.Equal(0, await db.AuditEvents.IgnoreQueryFilters().CountAsync(x =>
            x.OrganizationId == ownerOrg && x.Action == "contact.updated"));
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
