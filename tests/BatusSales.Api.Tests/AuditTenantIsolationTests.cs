using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Tests;

public sealed class AuditTenantIsolationTests
{
    [Fact]
    public async Task AuditReadsFailClosedAndRespectVerifiedTenantSwitchAndRevocation()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var userId = Guid.NewGuid();
        var organizationA = Guid.NewGuid();
        var organizationB = Guid.NewGuid();
        var auditA = new AuditEvent
        {
            OrganizationId = organizationA, ActorId = userId,
            Action = "contact.created", EntityId = Guid.NewGuid()
        };
        var auditB = new AuditEvent
        {
            OrganizationId = organizationB, ActorId = userId,
            Action = "contact.updated", EntityId = Guid.NewGuid()
        };

        await using var db = new AppDbContext(options);
        db.Memberships.AddRange(
            new OrganizationMembership
            {
                OrganizationId = organizationA, UserId = userId,
                Role = OrganizationRole.Owner
            },
            new OrganizationMembership
            {
                OrganizationId = organizationB, UserId = userId,
                Role = OrganizationRole.Owner
            });
        db.AuditEvents.AddRange(auditA, auditB);
        await db.SaveChangesAsync();

        // A context without current verified membership cannot list or guess audits.
        Assert.Empty(await db.AuditEvents.AsNoTracking().ToListAsync());
        Assert.Null(await db.AuditEvents.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == auditB.Id));

        var identity = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test"));

        Assert.True(await TenantAccess.IsMemberAsync(
            db, identity, organizationA, CancellationToken.None));
        var first = await db.AuditEvents.AsNoTracking().ToListAsync();
        Assert.Single(first);
        Assert.Equal(auditA.Id, first[0].Id);
        Assert.Null(await db.AuditEvents.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == auditB.Id));

        Assert.True(await TenantAccess.IsMemberAsync(
            db, identity, organizationB, CancellationToken.None));
        var second = await db.AuditEvents.AsNoTracking().ToListAsync();
        Assert.Single(second);
        Assert.Equal(auditB.Id, second[0].Id);

        Assert.False(await TenantAccess.IsMemberAsync(
            db, identity, Guid.NewGuid(), CancellationToken.None));
        Assert.Null(db.ActiveTenantId);
        Assert.Empty(await db.AuditEvents.AsNoTracking().ToListAsync());
    }
}
