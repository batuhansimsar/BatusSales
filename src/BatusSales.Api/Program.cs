using System.Security.Claims;
using System.Threading.RateLimiting;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Modules.Crm;
using BatusSales.Api.Modules.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "ConnectionStrings__Postgres must be configured; never commit database credentials.");

builder.Services.AddScoped<CurrentTenant>();
builder.Services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 12;
        options.User.RequireUniqueEmail = true;
        // Production release gate: set RequireConfirmedEmail = true and configure an email sender.
    })
    .AddEntityFrameworkStores<SalesDbContext>();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? http.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // Registration and login deserve a much smaller per-IP request budget.
    options.AddPolicy("auth", http =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

// For local development only. Production MUST use reviewed EF migrations,
// not EnsureCreated, which does not support schema evolution.
if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("BATUSSALES_DEV_CREATE_DB"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks("/health");
app.MapGroup("/auth").MapIdentityApi<ApplicationUser>().RequireRateLimiting("auth");
app.MapTenancyEndpoints();
app.MapCrmEndpoints();

app.Run();

public partial class Program
{
}
