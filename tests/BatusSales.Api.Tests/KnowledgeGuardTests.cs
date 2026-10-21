using System.Security.Claims;
using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Features.Knowledge;
using BatusSales.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Tests;

public sealed class KnowledgeGuardTests
{
    private static ClaimsPrincipal Principal(Guid id) => new(
        new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test"));

    [Theory]
    [InlineData(OrganizationRole.Owner, true)]
    [InlineData(OrganizationRole.Admin, true)]
    [InlineData(OrganizationRole.Agent, false)]
    public void OnlyOwnersAndAdminsMayEditFaq(OrganizationRole role, bool expected) =>
        Assert.Equal(expected, KnowledgeEndpoints.CanWrite(role));

    [Fact]
    public void InvalidInputsAreRejected()
    {
        Assert.Contains("question", KnowledgeValidation.Validate(new KnowledgeRequest(" ", "A")).Keys);
        Assert.Contains("answer", KnowledgeValidation.Validate(new KnowledgeRequest("Q", " ")).Keys);
        Assert.Contains("question",
            KnowledgeValidation.Validate(new KnowledgeRequest(new string('q', 301), "A")).Keys);
        Assert.Contains("answer",
            KnowledgeValidation.Validate(new KnowledgeRequest("Q", new string('a', 4001))).Keys);
        Assert.Empty(KnowledgeValidation.Validate(new KnowledgeRequest("Q", "A")));
    }

    [Fact]
    public async Task UnverifiedWritesFailClosedOnEverySavePath()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("DataSource=:memory:").Options;
        await using var db = new AppDbContext(options);
        db.KnowledgeEntries.Add(new KnowledgeEntry { OrganizationId = Guid.NewGuid(), Question = "Q", Answer = "A" });
        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges());
        Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges(false));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync(false));
    }

    [Fact]
    public async Task TenantFilterAndConcurrencyTokenBlockGuessedForeignId()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        var userA = Guid.NewGuid(); var userB = Guid.NewGuid(); var entryId = Guid.NewGuid();
        await using (var setup = new AppDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Users.AddRange(
                new AppUser { Id = userA, UserName = "faq-a", Email = "a@faq.test" },
                new AppUser { Id = userB, UserName = "faq-b", Email = "b@faq.test" });
            setup.Organizations.AddRange(new Organization { Id = a, Name = "A" }, new Organization { Id = b, Name = "B" });
            setup.Memberships.AddRange(
                new OrganizationMembership { OrganizationId = a, UserId = userA, Role = OrganizationRole.Owner },
                new OrganizationMembership { OrganizationId = b, UserId = userB, Role = OrganizationRole.Owner });
            await setup.SaveChangesAsync();
            Assert.True(await TenantAccess.IsMemberAsync(setup, Principal(userB), b, CancellationToken.None));
            setup.KnowledgeEntries.Add(new KnowledgeEntry
            { Id = entryId, OrganizationId = b, Question = "B", Answer = "B's private data" });
            await setup.SaveChangesAsync();
        }
        await using (var actor = new AppDbContext(options))
        {
            Assert.Empty(await actor.KnowledgeEntries.ToListAsync());
            Assert.True(await TenantAccess.IsMemberAsync(actor, Principal(userA), a, CancellationToken.None));
            Assert.Empty(await actor.KnowledgeEntries.ToListAsync());
            var forged = new KnowledgeEntry { Id = entryId, OrganizationId = a, Question = "A", Answer = "Attack" };
            actor.KnowledgeEntries.Attach(forged);
            actor.Entry(forged).Property(x => x.Answer).IsModified = true;
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => actor.SaveChangesAsync());
        }
        await using (var verify = new AppDbContext(options))
        {
            var persisted = await verify.KnowledgeEntries.IgnoreQueryFilters().SingleAsync(x => x.Id == entryId);
            Assert.Equal("B's private data", persisted.Answer);
            Assert.Equal(b, persisted.OrganizationId);
        }
    }
}
