using System.ComponentModel.DataAnnotations;
using BatusSales.Api.Auth;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Endpoints;

public static class ContactEndpoints
{
    public static void MapContactEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/contacts")
            .RequireAuthorization()
            .WithTags("CRM Contacts");

        group.AddEndpointFilter(async (invocation, next) =>
        {
            var http = invocation.HttpContext;
            var tenant = http.RequestServices.GetRequiredService<TenantContext>().TenantId;
            var subject = http.User.FindFirst("sub")?.Value;
            if (tenant is null || string.IsNullOrWhiteSpace(subject))
                return Results.Forbid();

            var db = http.RequestServices.GetRequiredService<AppDbContext>();
            var isMember = await db.TenantMemberships.AsNoTracking()
                .AnyAsync(x => x.TenantId == tenant.Value && x.SubjectId == subject,
                    http.RequestAborted);

            return isMember ? await next(invocation) : Results.Forbid();
        });

        group.MapGet("", async (AppDbContext db, int? limit, CancellationToken ct) =>
        {
            var size = Math.Clamp(limit ?? 50, 1, 100);
            var contacts = await db.Contacts.AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Take(size)
                .Select(x => new ContactResponse(x.Id, x.Name, x.Email, x.CreatedAt))
                .ToListAsync(ct);
            return Results.Ok(contacts);
        });

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var contact = await db.Contacts.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ContactResponse(x.Id, x.Name, x.Email, x.CreatedAt))
                .SingleOrDefaultAsync(ct);
            return contact is null ? Results.NotFound() : Results.Ok(contact);
        });

        group.MapPost("", async (CreateContactRequest request, TenantContext tenant,
            AppDbContext db, CancellationToken ct) =>
        {
            var name = request.Name?.Trim();
            var email = request.Email?.Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 160 ||
                name.Any(char.IsControl))
                return Results.BadRequest(new { error = "Name must contain 1-160 valid characters." });

            if (email is not null &&
                (email.Length > 320 || !new EmailAddressAttribute().IsValid(email)))
                return Results.BadRequest(new { error = "Invalid email address." });

            // The tenant ID is taken only from the authenticated request context.
            var contact = new Contact(tenant.TenantId!.Value, name, email);
            db.Contacts.Add(contact);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/contacts/{contact.Id}",
                new ContactResponse(contact.Id, contact.Name, contact.Email, contact.CreatedAt));
        }).RequireRateLimiting("contacts-write");
    }
}

public sealed record CreateContactRequest(string? Name, string? Email);
public sealed record ContactResponse(Guid Id, string Name, string? Email, DateTimeOffset CreatedAt);
