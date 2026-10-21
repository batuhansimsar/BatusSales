using System.Security.Claims;
using System.Threading.RateLimiting;
using BatusSales.Api.Auth;
using BatusSales.Api.Endpoints;
using BatusSales.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["Auth:Authority"];
var audience = builder.Configuration["Auth:Audience"];
var connectionString = builder.Configuration.GetConnectionString("Postgres");

if (string.IsNullOrWhiteSpace(authority)
    || !Uri.TryCreate(authority, UriKind.Absolute, out var issuer)
    || issuer.Scheme != Uri.UriSchemeHttps
    || string.IsNullOrWhiteSpace(audience)
    || string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure an HTTPS Auth:Authority, Auth:Audience, and ConnectionStrings:Postgres before starting the API.");
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TenantContext>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("contacts-write", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{context.User.FindFirstValue("sub") ?? "anonymous"}:{context.User.FindFirstValue("tenant_id") ?? "missing"}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "alive" }));
app.MapGet("/readyz", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    try
    {
        return await db.Database.CanConnectAsync(cancellationToken)
            ? Results.Ok(new { status = "ready" })
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapContactEndpoints();

app.Run();

public partial class Program { }
