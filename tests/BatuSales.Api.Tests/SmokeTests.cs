using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BatuSales.Api.Tests;

public sealed class SmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SmokeTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Liveness_Is_Ok()
    {
        using var response = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Status_Reports_Foundation_Phase()
    {
        using var response = await _client.GetAsync("/api/v1/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("foundation", json.GetProperty("phase").GetString());
    }

    [Fact]
    public async Task Unknown_Endpoint_Is_Not_Found()
    {
        using var response = await _client.GetAsync("/api/v1/contacts");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
