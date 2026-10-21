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

namespace BatusSales.Api.IntegrationTests;

/// <summary>
/// Runs against an actual disposable PostgreSQL database, not EF InMemory.
/// The CI job provides BATUSSALES_TEST_POSTGRES_CONNECTION.
/// </summary>
public sealed class TenantBoundaryTests
{
    [Fact]
    public async Task AuthenticatedContactsAreTenantScopedAndMembershipRevocationTakesEffect()
    {
        await using var factory = new PostgresAppFactory();
        var clientOptions = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        };
        using var alice = factory.CreateClient(clientOptions);
        using var bob = factory.CreateClient(clientOptions);
        using var anonymous = factory.CreateClient(clientOptions);

        await RegisterAndLoginAsync(alice);
        await RegisterAndLoginAsync(bob);

        var aliceOrganization = await CreateOrganizationAsync(alice, "Alice Business");
        var bobOrganization = await CreateOrganizationAsync(bob, "Bob Business");

        var alicePath = $"/api/organizations/{aliceOrganization}/contacts";
        var bobPath = $"/api/organizations/{bobOrganization}/contacts";

        using var aliceCreated = await alice.PostAsJsonAsync(
            alicePath, new { name = "Alice Contact", email = "alice-contact@example.com", phone = (string?)null });
        Assert.Equal(HttpStatusCode.Created, aliceCreated.StatusCode);
        var aliceContactId = await ReadGuidAsync(aliceCreated, "id");

        using var bobCreated = await bob.PostAsJsonAsync(
            bobPath, new { name = "Bob Contact", email = "bob-contact@example.com", phone = (string?)null });
        Assert.Equal(HttpStatusCode.Created, bobCreated.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(alicePath)).StatusCode);

        // A valid identity for another organization is not sufficient authorization.
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync(alicePath)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await alice.GetAsync(bobPath)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await bob.GetAsync($"{alicePath}/{aliceContactId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await bob.PostAsJsonAsync(alicePath, new { name = "Unauthorized" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await bob.PutAsJsonAsync($"{alicePath}/{aliceContactId}", new { name = "Unauthorized" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await bob.DeleteAsync($"{alicePath}/{aliceContactId}")).StatusCode);

        using (var ownList = await alice.GetAsync(alicePath))
        {
            Assert.Equal(HttpStatusCode.OK, ownList.StatusCode);
            using var json = JsonDocument.Parse(await ownList.Content.ReadAsStringAsync());
            var items = json.RootElement.GetProperty("items");
            Assert.Equal(1, items.GetArrayLength());
            Assert.Equal(aliceContactId, items[0].GetProperty("id").GetGuid());
        }

        // Revoke membership directly in PostgreSQL. A previously issued token must
        // no longer authorize CRM access on the very next HTTP request.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var membership = await db.Memberships.SingleAsync(
                x => x.OrganizationId == aliceOrganization);
            db.Memberships.Remove(membership);
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.NotFound, (await alice.GetAsync(alicePath)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await alice.GetAsync($"{alicePath}/{aliceContactId}")).StatusCode);
    }

    private static async Task RegisterAndLoginAsync(HttpClient client)
    {
        var email = $"tenant-{Guid.NewGuid():N}@example.com";
        const string password = "DevelopmentOnly-Strong#Pass123";

        using var registered = await client.PostAsJsonAsync("/auth/register", new { email, password });
        Assert.Equal(HttpStatusCode.OK, registered.StatusCode);

        using var login = await client.PostAsJsonAsync(
            "/auth/login?useCookies=false", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var accessToken = json.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private static async Task<Guid> CreateOrganizationAsync(HttpClient client, string name)
    {
        using var created = await client.PostAsJsonAsync("/api/organizations", new { name });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        return await ReadGuidAsync(created, "id");
    }

    private static async Task<Guid> ReadGuidAsync(HttpResponseMessage response, string field)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty(field).GetGuid();
    }

    private sealed class PostgresAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connection = Environment.GetEnvironmentVariable("BATUSSALES_TEST_POSTGRES_CONNECTION");
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException(
                    "BATUSSALES_TEST_POSTGRES_CONNECTION is required; use an isolated disposable test database.");

            // The current foundation intentionally supports database bootstrap only in
            // Development. This is NOT a substitute for EF migrations.
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = connection
                }));
        }
    }
}
