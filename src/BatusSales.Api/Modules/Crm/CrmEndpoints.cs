using System.ComponentModel.DataAnnotations;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Modules.Crm;

public static class CrmEndpoints
{
    public static IEndpointRouteBuilder MapCrmEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/organizations/{organizationId:guid}/contacts")
            .RequireAuthorization()
            .AddEndpointFilter<TenantAccessFilter>();

        group.MapGet("", ListContacts);
        group.MapGet("/{contactId:guid}", GetContact);
        group.MapPost("", CreateContact);
        return endpoints;
    }

    private static async Task<IResult> ListContacts(
        int? page,
        int? pageSize,
        SalesDbContext db,
        CancellationToken cancellationToken)
    {
        var currentPage = Math.Clamp(page ?? 1, 1, 1_000_000);
        var take = Math.Clamp(pageSize ?? 20, 1, 100);
        var query = db.Contacts.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Id)
            .Skip((currentPage - 1) * take)
            .Take(take)
            .Select(c => new ContactResponse(c.Id, c.Name, c.Email, c.Phone, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(new { items = rows, total, page = currentPage, pageSize = take });
    }

    private static async Task<IResult> GetContact(
        Guid contactId,
        SalesDbContext db,
        CancellationToken cancellationToken)
    {
        var contact = await db.Contacts.AsNoTracking()
            .Where(c => c.Id == contactId)
            .Select(c => new ContactResponse(c.Id, c.Name, c.Email, c.Phone, c.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return contact is null ? Results.NotFound() : Results.Ok(contact);
    }

    private static async Task<IResult> CreateContact(
        ContactRequest input,
        CurrentTenant tenant,
        SalesDbContext db,
        CancellationToken cancellationToken)
    {
        var name = input.Name?.Trim();
        var email = input.Email?.Trim();
        var phone = input.Phone?.Trim();

        if (string.IsNullOrEmpty(name) || name.Length > 160)
            return Results.BadRequest(new { error = "Name must be 1-160 characters." });
        if (email is { Length: > 254 } ||
            (email is { Length: > 0 } && !new EmailAddressAttribute().IsValid(email)))
            return Results.BadRequest(new { error = "Invalid email address." });
        if (phone is { Length: > 32 })
            return Results.BadRequest(new { error = "Phone cannot exceed 32 characters." });

        var contact = new Contact
        {
            OrganizationId = tenant.Id!.Value, // Set only by TenantAccessFilter.
            Name = name,
            Email = string.IsNullOrEmpty(email) ? null : email,
            Phone = string.IsNullOrEmpty(phone) ? null : phone
        };

        db.Contacts.Add(contact);
        await db.SaveChangesAsync(cancellationToken);
        var response = new ContactResponse(
            contact.Id, contact.Name, contact.Email, contact.Phone, contact.CreatedAt);
        return Results.Created(
            $"/api/organizations/{contact.OrganizationId}/contacts/{contact.Id}", response);
    }

    public sealed record ContactRequest(string? Name, string? Email, string? Phone);
    public sealed record ContactResponse(
        Guid Id, string Name, string? Email, string? Phone, DateTimeOffset CreatedAt);
}
