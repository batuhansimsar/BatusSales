using BatusSales.Api.Data;
using BatusSales.Api.Security;
using BatusSales.Api.Validation;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Endpoints;

public static class ContactsEndpoints
{
    public static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations/{organizationId:guid}/contacts")
            .RequireAuthorization();

        group.MapGet("/", async (Guid organizationId, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var userId = TenantAccess.GetUserId(http.User);
            if (userId is null) return Results.Unauthorized();
            if (await TenantAccess.GetMembershipAsync(db, organizationId, userId, ct) is null)
                return Results.NotFound();

            var contacts = await db.Contacts.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId)
                .OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
                .Take(50)
                .Select(x => new ContactResponse(x.Id, x.Name, x.Email, x.Phone, x.CreatedAt))
                .ToListAsync(ct);
            return Results.Ok(contacts);
        });

        group.MapGet("/{contactId:guid}", async (Guid organizationId, Guid contactId, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var userId = TenantAccess.GetUserId(http.User);
            if (userId is null) return Results.Unauthorized();
            if (await TenantAccess.GetMembershipAsync(db, organizationId, userId, ct) is null)
                return Results.NotFound();

            var contact = await db.Contacts.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId && x.Id == contactId)
                .Select(x => new ContactResponse(x.Id, x.Name, x.Email, x.Phone, x.CreatedAt))
                .SingleOrDefaultAsync(ct);
            return contact is null ? Results.NotFound() : Results.Ok(contact);
        });

        group.MapPost("/", async (Guid organizationId, ContactInput input, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var userId = TenantAccess.GetUserId(http.User);
            if (userId is null) return Results.Unauthorized();
            if (await TenantAccess.GetMembershipAsync(db, organizationId, userId, ct) is null)
                return Results.NotFound();

            var errors = ContactInputValidator.Validate(input);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var contact = new Contact
            {
                OrganizationId = organizationId,
                Name = input.Name!.Trim(),
                Email = Normalize(input.Email),
                Phone = Normalize(input.Phone)
            };
            db.Contacts.Add(contact);
            db.AuditEvents.Add(new AuditEvent
            {
                OrganizationId = organizationId,
                ActorUserId = userId,
                Action = "contact.created",
                EntityId = contact.Id
            });
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/organizations/{organizationId}/contacts/{contact.Id}",
                ToResponse(contact));
        });

        group.MapPut("/{contactId:guid}", async (Guid organizationId, Guid contactId, ContactInput input, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var userId = TenantAccess.GetUserId(http.User);
            if (userId is null) return Results.Unauthorized();
            if (await TenantAccess.GetMembershipAsync(db, organizationId, userId, ct) is null)
                return Results.NotFound();

            var errors = ContactInputValidator.Validate(input);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            // Both tenant key and entity ID are mandatory; never load by contact ID alone.
            var contact = await db.Contacts.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == contactId, ct);
            if (contact is null) return Results.NotFound();

            contact.Name = input.Name!.Trim();
            contact.Email = Normalize(input.Email);
            contact.Phone = Normalize(input.Phone);
            db.AuditEvents.Add(new AuditEvent
            {
                OrganizationId = organizationId,
                ActorUserId = userId,
                Action = "contact.updated",
                EntityId = contact.Id
            });
            await db.SaveChangesAsync(ct);
            return Results.Ok(ToResponse(contact));
        });

        return app;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ContactResponse ToResponse(Contact contact) =>
        new(contact.Id, contact.Name, contact.Email, contact.Phone, contact.CreatedAt);

    public sealed record ContactResponse(Guid Id, string Name, string? Email, string? Phone, DateTimeOffset CreatedAt);
}
