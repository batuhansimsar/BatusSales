using System.Threading.RateLimiting;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Features.Contacts;
using BatusSales.Api.Features.Organizations;
using BatusSales.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:Default is required. See README.md.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
// Restrict tenant/business APIs to bearer credentials until a CSRF-safe browser BFF exists.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BatusSalesBearer", policy =>
        policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme)
            .RequireAuthenticatedUser());
});
builder.Services.AddIdentityApiEndpoints<AppUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.SignIn.RequireConfirmedEmail = false; // Development only.
}).AddEntityFrameworkStores<AppDbContext>();

// Production must have real email delivery and confirmed-email login.
if (builder.Environment.IsDevelopment())
    builder.Services.AddTransient<IEmailSender<AppUser>, DevelopmentEmailSender>();
else
    throw new InvalidOperationException("Production email and verification are not configured.");

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
}));

app.UseHttpsRedirection();

// MapIdentityApi supports query-controlled cookie sign-in. Disable that mode
// until an explicit browser-session and antiforgery design has been reviewed.
app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method) &&
        string.Equals(context.Request.Path.Value, "/auth/login", StringComparison.OrdinalIgnoreCase) &&
        context.Request.Query.Any(parameter =>
            (string.Equals(parameter.Key, "useCookies", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(parameter.Key, "useSessionCookies", StringComparison.OrdinalIgnoreCase)) &&
            !string.Equals(parameter.Value.ToString(), "false", StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Cookie sign-in is unavailable until CSRF protection is implemented."
        });
        return;
    }

    await next(context);
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/ready", async (AppDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapGroup("/auth").RequireRateLimiting("auth").MapIdentityApi<AppUser>();
app.MapOrganizationEndpoints();
app.MapContactEndpoints();

app.Run();

public partial class Program { }
