using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// The HTTP suites currently share one physical PostgreSQL test database.
// Keep distinct WebApplicationFactory instances from racing on EnsureCreated.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BatusSales.Api.IntegrationTests;

public sealed class ContactRoleHttpTests(PostgreSqlApiFactory factory)
    : IClassFixture<PostgreSqlApiFactory>
{
    [Fact]
    public async Task ContactDeleteRequiresCurrentDatabaseRoleAndDenialLeavesNoAudit()
    {
        using var owner = CreateClient();
        using var agent = CreateClient();
        using var outsider = CreateClient();

        await AuthenticateAsync(owner);
        var agentEmail = await AuthenticateAsync(agent);
        await AuthenticateAsync(outsider);

        var organizationResponse = await owner.PostAsJsonAsync(
            "/api/organizations", new { name = "Role authorization integration test" });
        Assert.Equal(HttpStatusCode.Created, organizationResponse.StatusCode);
        var organizationId = (await organizationResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        var contactPath = $"/api/organizations/{organizationId}/contacts";
        var createResponse = await owner.PostAsJsonAsync(contactPath,
            new { name = "Protected contact", email = "protected@example.test", phone = (string?)null });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var contactId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();
        var resourcePath = $"{contactPath}/{contactId}";

        Guid agentId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            agentId = await db.Users.AsNoTracking()
                .Where(x => x.Email == agentEmail)
                .Select(x => x.Id).SingleAsync();
            db.Memberships.Add(new OrganizationMembership
            {
                OrganizationId = organizationId,
                UserId = agentId,
                Role = OrganizationRole.Agent
            });
            await db.SaveChangesAsync();
        }

        // The Agent is an actual member and may view the contact, but not delete it.
        Assert.Equal(HttpStatusCode.OK, (await agent.GetAsync(resourcePath)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outsider.DeleteAsync(resourcePath)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await agent.DeleteAsync(resourcePath)).StatusCode);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Test-only verification: never bypass query filters in business endpoints.
            var contact = await db.Contacts.IgnoreQueryFilters().AsNoTracking()
                .SingleAsync(x => x.Id == contactId && x.OrganizationId == organizationId);
            Assert.False(contact.IsDeleted);
            Assert.Equal(0, await db.AuditEvents.CountAsync(x =>
                x.OrganizationId == organizationId && x.EntityId == contactId &&
                x.Action == "contact.deleted"));

            // Change the authoritative role without refreshing the same bearer token.
            var membership = await db.Memberships.SingleAsync(
                x => x.OrganizationId == organizationId && x.UserId == agentId);
            membership.Role = OrganizationRole.Admin;
            await db.SaveChangesAsync();
        }

        // A fresh database membership lookup must now permit the same user's request.
        Assert.Equal(HttpStatusCode.NoContent, (await agent.DeleteAsync(resourcePath)).StatusCode);

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True((await verifyDb.Contacts.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(x => x.Id == contactId && x.OrganizationId == organizationId)).IsDeleted);
        Assert.Equal(1, await verifyDb.AuditEvents.CountAsync(x =>
            x.OrganizationId == organizationId && x.EntityId == contactId &&
            x.ActorId == agentId && x.Action == "contact.deleted"));
    }

    private HttpClient CreateClient() =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

    private static async Task<string> AuthenticateAsync(HttpClient client)
    {
        var email = $"role-{Guid.NewGuid():N}@example.test";
        const string password = "Strong-Integration-Password_2026!";

        var registration = await client.PostAsJsonAsync("/auth/register",
            new { email, password });
        Assert.True(registration.IsSuccessStatusCode,
            $"Register returned {(int)registration.StatusCode}: " +
            await registration.Content.ReadAsStringAsync());

        var login = await client.PostAsJsonAsync("/auth/login?useCookies=false",
            new { email, password });
        Assert.True(login.IsSuccessStatusCode,
            $"Login returned {(int)login.StatusCode}: " +
            await login.Content.ReadAsStringAsync());

        var payload = await login.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", payload.GetProperty("accessToken").GetString());
        return email;
    }
}
