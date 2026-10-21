using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// This is a coarse unauthenticated baseline, NOT tenant-aware usage metering.
// Identity/Tenancy will add authenticated per-tenant limits.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();

app.MapHealthChecks("/health/live");
app.MapGet("/api/v1/meta", () => Results.Ok(new
{
    application = "BatusSales",
    apiVersion = "v1",
    stage = "foundation"
}));

// No user, customer, organization, booking, or AI routes are exposed until
// authentication, authorization and tenant boundary tests are implemented.
app.Run();

public partial class Program;
