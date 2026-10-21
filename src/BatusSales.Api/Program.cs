using BatusSales.Api.Data;
using BatusSales.Api.Tenancy;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(services => services.GetRequiredService<TenantContext>());

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Missing ConnectionStrings:Postgres. Supply it through a secret or environment variable.");

builder.Services.AddDbContext<BatusSalesDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseExceptionHandler();

// Liveness does not depend on external services.
app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }))
    .WithName("Liveness");

// Readiness fails closed when the backing datastore is unavailable.
app.MapGet("/health/ready", async (
    BatusSalesDbContext database,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (await database.Database.CanConnectAsync(cancellationToken))
            return Results.Ok(new { status = "ready" });
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        logger.LogWarning(ex, "Database readiness check failed");
    }

    return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
}).WithName("Readiness");

// Deliberately no organization or CRM endpoints yet: authentication,
// membership authorization and migrations must land before data exposure.
app.Run();

public partial class Program;
