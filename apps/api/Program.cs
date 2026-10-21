using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BatuSales.Api.Data;
using BatuSales.Api.Domain;
using BatuSales.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Fail closed: authenticated API routes must never run without a real issuer,
// audience and database connection.
var authority = builder.Configuration["Auth:Authority"];
var audience = builder.Configuration["Auth:Audience"];
var connectionString = builder.Configuration.GetConnectionString("BatuSales");

if (string.IsNullOrWhiteSpace(authority)
    || !Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri)
    || authorityUri.Scheme != Uri.UriSchemeHttps)
    throw new InvalidOperationException("Auth:Authority must be a valid HTTPS OIDC issuer.");

if (string.IsNullOrWhiteSpace(audience))
    throw new InvalidOperationException("Auth:Audience is required.");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:BatuSales is required.");

builder.Services.AddProblemDetails();
builder.Services.AddScoped<TenantScope>();
builder.Services.AddScoped<TenantAccess>();
builder.Services.AddDbContext<BatuSalesDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateLifetime = true;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }))
    .AllowAnonymous();
app.MapGet("/health/ready", async (BatuSalesDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct)
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable))
    .AllowAnonymous();

var api = app.MapGroup("/api").RequireAuthorization();

api.MapGet("/organizations", async (ClaimsPrincipal user, BatuSalesDbContext db, CancellationToken ct) =>
{
    var subject = user.FindFirst("sub")?.Value;
    if (string.IsNullOrWhiteSpace(subject)) return Results.Unauthorized();

    var organizations = await db.Members.AsNoTracking()
        .Where(m => m.UserId == subject)
        .Join(db.Organizations.AsNoTracking(),
            m => m.OrganizationId, o => o.Id,
            (m, o) => new { id = o.Id, name = o.Name, role = m.Role })
        .OrderBy(o => o.name).ToListAsync(ct);

    return Results.Ok(organizations);
});

api.MapPost("/organizations", async (
    CreateOrganizationRequest request,
    ClaimsPrincipal user,
    BatuSalesDbContext db,
    CancellationToken ct) =>
{
    var subject = user.FindFirst("sub")?.Value;
    if (string.IsNullOrWhiteSpace(subject)) return Results.Unauthorized();

    var name = request.Name?.Trim();
    if (string.IsNullOrWhiteSpace(name) || name.Length > 120)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["name"] = ["Organization name must be 1-120 characters."]
        });

    var id = Guid.NewGuid();
    var now = DateTimeOffset.UtcNow;
    db.Organizations.Add(new Organization { Id = id, Name = name, CreatedAt = now });
    db.Members.Add(new OrganizationMember
    {
        OrganizationId = id, UserId = subject,
        Role = OrganizationRoles.Owner, CreatedAt = now
    });
    // EF Core wraps multi-statement relational SaveChanges in a transaction.
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/organizations/{id}", new { id, name, role = OrganizationRoles.Owner });
});

api.MapGet("/organizations/{organizationId:guid}", async (
    Guid organizationId, ClaimsPrincipal user, BatuSalesDbContext db, CancellationToken ct) =>
{
    var subject = user.FindFirst("sub")?.Value;
    if (string.IsNullOrWhiteSpace(subject)) return Results.Unauthorized();

    var details = await db.Members.AsNoTracking()
        .Where(m => m.OrganizationId == organizationId && m.UserId == subject)
        .Join(db.Organizations.AsNoTracking(),
            m => m.OrganizationId, o => o.Id,
            (m, o) => new { id = o.Id, name = o.Name, role = m.Role, createdAt = o.CreatedAt })
        .SingleOrDefaultAsync(ct);

    return details is null ? Results.NotFound() : Results.Ok(details);
});

api.MapGet("/organizations/{organizationId:guid}/contacts", async (
    Guid organizationId, ClaimsPrincipal user, TenantAccess access,
    BatuSalesDbContext db, HttpRequest http, CancellationToken ct) =>
{
    if (!await access.TryEnterAsync(organizationId, user, ct)) return Results.NotFound();

    var page = int.TryParse(http.Query["page"].ToString(), out var requestedPage)
        ? Math.Clamp(requestedPage, 1, 100000) : 1;
    var size = int.TryParse(http.Query["pageSize"].ToString(), out var requestedSize)
        ? Math.Clamp(requestedSize, 1, 100) : 20;

    var contacts = await db.Contacts.AsNoTracking()
        .OrderByDescending(c => c.CreatedAt)
        .ThenByDescending(c => c.Id)
        .Skip((page - 1) * size).Take(size)
        .Select(c => new { c.Id, c.FullName, c.Email, c.Phone, c.Notes, c.CreatedAt })
        .ToListAsync(ct);
    return Results.Ok(new { page, pageSize = size, items = contacts });
});

api.MapGet("/organizations/{organizationId:guid}/contacts/{contactId:guid}", async (
    Guid organizationId, Guid contactId, ClaimsPrincipal user,
    TenantAccess access, BatuSalesDbContext db, CancellationToken ct) =>
{
    if (!await access.TryEnterAsync(organizationId, user, ct)) return Results.NotFound();

    var contact = await db.Contacts.AsNoTracking()
        .Where(c => c.Id == contactId)
        .Select(c => new { c.Id, c.FullName, c.Email, c.Phone, c.Notes, c.CreatedAt })
        .SingleOrDefaultAsync(ct);
    return contact is null ? Results.NotFound() : Results.Ok(contact);
});

api.MapPost("/organizations/{organizationId:guid}/contacts", async (
    Guid organizationId, CreateContactRequest request, ClaimsPrincipal user,
    TenantAccess access, BatuSalesDbContext db, CancellationToken ct) =>
{
    if (!await access.TryEnterAsync(organizationId, user, ct)) return Results.NotFound();

    var name = request.FullName?.Trim();
    var email = request.Email?.Trim();
    var phone = request.Phone?.Trim();
    var notes = request.Notes?.Trim();
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(name) || name.Length > 160)
        errors["fullName"] = ["Contact name must be 1-160 characters."];
    if (!string.IsNullOrEmpty(email) && (email.Length > 254 || !new EmailAddressAttribute().IsValid(email)))
        errors["email"] = ["Email must be valid and at most 254 characters."];
    if (phone?.Length > 32) errors["phone"] = ["Phone must be at most 32 characters."];
    if (notes?.Length > 2000) errors["notes"] = ["Notes must be at most 2000 characters."];
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    var contact = new Contact
    {
        Id = Guid.NewGuid(),
        OrganizationId = organizationId,
        FullName = name!,
        Email = string.IsNullOrEmpty(email) ? null : email,
        Phone = string.IsNullOrEmpty(phone) ? null : phone,
        Notes = string.IsNullOrEmpty(notes) ? null : notes,
        CreatedAt = DateTimeOffset.UtcNow
    };

    db.Contacts.Add(contact);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/organizations/{organizationId}/contacts/{contact.Id}",
        new { contact.Id, contact.FullName, contact.Email, contact.Phone, contact.Notes, contact.CreatedAt });
});

app.Run();

// Enables WebApplicationFactory integration tests in a later milestone.
public partial class Program { }
