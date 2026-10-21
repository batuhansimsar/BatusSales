using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                QueueLimit = 0,
                AutoReplenishment = true,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    await next();
});
app.UseRateLimiter();

app.MapHealthChecks("/health/live").WithTags("System");
app.MapGet("/api/v1/system/info", () =>
        TypedResults.Ok(new SystemInfo("BatusSales", "v1", "foundation")))
    .WithName("GetSystemInfo")
    .WithTags("System");

app.Run();

public sealed record SystemInfo(string Product, string ApiVersion, string Phase);
public partial class Program { }
