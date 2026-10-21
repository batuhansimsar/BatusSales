using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Features.Knowledge;

public sealed record KnowledgeRequest(string? Question, string? Answer);
public sealed record KnowledgeResponse(
    Guid Id, Guid OrganizationId, string Question, string Answer,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc)
{
    public static KnowledgeResponse From(KnowledgeEntry entry) =>
        new(entry.Id, entry.OrganizationId, entry.Question, entry.Answer,
            entry.CreatedAtUtc, entry.UpdatedAtUtc);
}

public static class KnowledgeValidation
{
    public static Dictionary<string, string[]> Validate(KnowledgeRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Question) || request.Question.Trim().Length > 300)
            errors["question"] = ["Question must be between 1 and 300 characters."];
        if (string.IsNullOrWhiteSpace(request.Answer) || request.Answer.Trim().Length > 4000)
            errors["answer"] = ["Answer must be between 1 and 4000 characters."];
        return errors;
    }
}

/// <summary>FAQ for customers: exact membership to read, database Owner/Admin role to write.</summary>
public static class KnowledgeEndpoints
{
    public static bool CanWrite(OrganizationRole? role) =>
        role is OrganizationRole.Owner or OrganizationRole.Admin;

    public static void MapKnowledgeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organizations/{organizationId:guid}/knowledge/faqs")
            .RequireAuthorization();

        group.MapGet("", async (
            Guid organizationId, int? page, int? pageSize, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            if (!await TenantAccess.IsMemberAsync(db, context.User, organizationId, ct))
                return Results.NotFound();
            var currentPage = Math.Clamp(page ?? 1, 1, 100_000);
            var size = Math.Clamp(pageSize ?? 25, 1, 100);
            var query = db.KnowledgeEntries.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId);
            var count = await query.CountAsync(ct);
            var items = await query.OrderByDescending(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id).Skip((currentPage - 1) * size).Take(size)
                .Select(x => new KnowledgeResponse(
                    x.Id, x.OrganizationId, x.Question, x.Answer,
                    x.CreatedAtUtc, x.UpdatedAtUtc)).ToListAsync(ct);
            return Results.Ok(new { items, page = currentPage, pageSize = size, total = count });
        });

        group.MapGet("/{entryId:guid}", async (
            Guid organizationId, Guid entryId, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            if (!await TenantAccess.IsMemberAsync(db, context.User, organizationId, ct))
                return Results.NotFound();
            var entry = await db.KnowledgeEntries.AsNoTracking()
                .SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == entryId, ct);
            return entry is null ? Results.NotFound() : Results.Ok(KnowledgeResponse.From(entry));
        });

        group.MapPost("", async (
            Guid organizationId, KnowledgeRequest input, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            var role = await TenantAccess.GetRoleAsync(db, context.User, organizationId, ct);
            if (role is null) return Results.NotFound();
            if (!CanWrite(role)) return Results.Forbid();
            var errors = KnowledgeValidation.Validate(input);
            if (errors.Count != 0) return Results.ValidationProblem(errors);
            TenantAccess.TryGetUserId(context.User, out var userId);
            var entry = new KnowledgeEntry
            {
                OrganizationId = organizationId,
                Question = input.Question!.Trim(),
                Answer = input.Answer!.Trim()
            };
            db.KnowledgeEntries.Add(entry);
            db.AuditEvents.Add(NewAudit(organizationId, userId, "knowledge.created", entry.Id));
            await db.SaveChangesAsync(ct);
            return Results.Created(
                $"/api/organizations/{organizationId}/knowledge/faqs/{entry.Id}",
                KnowledgeResponse.From(entry));
        });

        group.MapPut("/{entryId:guid}", async (
            Guid organizationId, Guid entryId, KnowledgeRequest input,
            HttpContext context, AppDbContext db, CancellationToken ct) =>
        {
            var role = await TenantAccess.GetRoleAsync(db, context.User, organizationId, ct);
            if (role is null) return Results.NotFound();
            if (!CanWrite(role)) return Results.Forbid();
            var errors = KnowledgeValidation.Validate(input);
            if (errors.Count != 0) return Results.ValidationProblem(errors);
            var entry = await db.KnowledgeEntries.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == entryId, ct);
            if (entry is null) return Results.NotFound();
            TenantAccess.TryGetUserId(context.User, out var userId);
            entry.Question = input.Question!.Trim();
            entry.Answer = input.Answer!.Trim();
            entry.UpdatedAtUtc = DateTime.UtcNow;
            db.AuditEvents.Add(NewAudit(organizationId, userId, "knowledge.updated", entry.Id));
            await db.SaveChangesAsync(ct);
            return Results.Ok(KnowledgeResponse.From(entry));
        });

        group.MapDelete("/{entryId:guid}", async (
            Guid organizationId, Guid entryId, HttpContext context,
            AppDbContext db, CancellationToken ct) =>
        {
            var role = await TenantAccess.GetRoleAsync(db, context.User, organizationId, ct);
            if (role is null) return Results.NotFound();
            if (!CanWrite(role)) return Results.Forbid();
            var entry = await db.KnowledgeEntries.SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == entryId, ct);
            if (entry is null) return Results.NotFound();
            TenantAccess.TryGetUserId(context.User, out var userId);
            entry.IsDeleted = true;
            entry.UpdatedAtUtc = DateTime.UtcNow;
            db.AuditEvents.Add(NewAudit(organizationId, userId, "knowledge.deleted", entry.Id));
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static AuditEvent NewAudit(Guid organizationId, Guid userId, string action, Guid entityId) =>
        new() { OrganizationId = organizationId, ActorId = userId, Action = action, EntityId = entityId };
}
