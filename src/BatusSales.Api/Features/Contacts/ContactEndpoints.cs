using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Features.Contacts;

public static class ContactEndpoints
{
    public static void MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations/{organizationId:guid}/contacts")
            .RequireAuthorization();

        group.MapGet("", async (
            Guid organizationId, int? page, int? pageSize, string? search, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            if (!await TenantAccess.IsMemberAsync(db, context.User, organizationId, ct))
                return Results.NotFound();

            // Search only after membership resolution; keep the tenant predicate on
            // the server and treat user-supplied LIKE metacharacters literally.
            var term = search?.Trim();
            if (term is { Length: > 100 })
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["search"] = ["Search must be at most 100 characters."]
                });

            var currentPage = Math.Clamp(page ?? 1, 1, 100_000);
            var size = Math.Clamp(pageSize ?? 25, 1, 100);
            IQueryable<Contact> query = db.Contacts.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId);
            if (!string.IsNullOrWhiteSpace(term))
            {
                var literal = term.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
                var pattern = $"%{literal}%";
                query = query.Where(x =>
                    EF.Functions.ILike(x.Name, pattern, @"\") ||
                    (x.Email != null && EF.Functions.ILike(x.Email, pattern, @"\")) ||
                    (x.Phone != null && EF.Functions.ILike(x.Phone, pattern, @"\")));
            }

            var count = await query.CountAsync(ct);
            var items = await query.OrderByDescending(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id)
                .Skip((currentPage - 1) * size).Take(size)
                .Select(x => new ContactResponse(
                    x.Id, x.OrganizationId, x.Name, x.Email, x.Phone,
                    x.CreatedAtUtc, x.UpdatedAtUtc))
                .ToListAsync(ct);

            return Results.Ok(new { items, page = currentPage, pageSize = size, total = count });
        });

        group.MapGet("/{contactId:guid}", async (
            Guid organizationId, Guid contactId, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            if (!await TenantAccess.IsMemberAsync(db, context.User, organizationId, ct))
                return Results.NotFound();

            var contact = await db.Contacts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == contactId, ct);

            return contact is null ? Results.NotFound() : Results.Ok(ContactResponse.From(contact));
        });

        group.MapPost("", async (
            Guid organizationId, ContactRequest input, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            if (!await TenantAccess.IsMemberAsync(db, context.User, organizationId, ct))
                return Results.NotFound();

            var errors = ContactValidation.Validate(input);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            TenantAccess.TryGetUserId(context.User, out var userId);
            var contact = new Contact
            {
                OrganizationId = organizationId,
                Name = input.Name!.Trim(),
                Email = NullIfBlank(input.Email),
                Phone = NullIfBlank(input.Phone)
            };
            db.Contacts.Add(contact);
            db.AuditEvents.Add(NewAudit(organizationId, userId, "contact.created", contact.Id));
            await db.SaveChangesAsync(ct);

            return Results.Created(
                $"/api/organizations/{organizationId}/contacts/{contact.Id}",
                ContactResponse.From(contact));
        });

        group.MapPut("/{contactId:guid}", async (
            Guid organizationId, Guid contactId, ContactRequest input, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            if (!await TenantAccess.IsMemberAsync(db, context.User, organizationId, ct))
                return Results.NotFound();

            var errors = ContactValidation.Validate(input);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var contact = await db.Contacts.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == contactId, ct);
            if (contact is null) return Results.NotFound();

            TenantAccess.TryGetUserId(context.User, out var userId);
            contact.Name = input.Name!.Trim();
            contact.Email = NullIfBlank(input.Email);
            contact.Phone = NullIfBlank(input.Phone);
            contact.UpdatedAtUtc = DateTime.UtcNow;
            db.AuditEvents.Add(NewAudit(organizationId, userId, "contact.updated", contact.Id));
            await db.SaveChangesAsync(ct);

            return Results.Ok(ContactResponse.From(contact));
        });

        group.MapDelete("/{contactId:guid}", async (
            Guid organizationId, Guid contactId, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            var role = await TenantAccess.GetRoleAsync(db, context.User, organizationId, ct);
            if (role is null)
                return Results.NotFound();
            if (!TenantAccess.CanDeleteContacts(role))
                return Results.Forbid();

            var contact = await db.Contacts.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == contactId, ct);
            if (contact is null) return Results.NotFound();

            TenantAccess.TryGetUserId(context.User, out var userId);
            contact.IsDeleted = true;
            contact.UpdatedAtUtc = DateTime.UtcNow;
            db.AuditEvents.Add(NewAudit(organizationId, userId, "contact.deleted", contact.Id));
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        });
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AuditEvent NewAudit(Guid organizationId, Guid userId, string action, Guid id) =>
        new()
        {
            OrganizationId = organizationId, ActorId = userId, Action = action, EntityId = id
        };
}
