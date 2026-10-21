using BatusSales.Api.Data;
using BatusSales.Api.Domain;
using BatusSales.Api.Tenancy;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BatusSales.Api.Tests;

public sealed class TenantIsolationTests
{
    [Fact]
    public async Task Tenant_queries_cannot_see_other_tenants_records()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<BatusSalesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var aContext = new TenantContext();
        aContext.InitializeAfterAuthorization(a);
        await using (var db = new BatusSalesDbContext(options, aContext))
        {
            db.Contacts.Add(new Contact { DisplayName = "Tenant A", OrganizationId = a });
            await db.SaveChangesAsync();
        }

        var bContext = new TenantContext();
        bContext.InitializeAfterAuthorization(b);
        await using (var db = new BatusSalesDbContext(options, bContext))
        {
            Assert.Empty(await db.Contacts.ToListAsync());
        }
    }

    [Fact]
    public async Task Cross_tenant_writes_are_rejected()
    {
        var context = new TenantContext();
        context.InitializeAfterAuthorization(Guid.NewGuid());
        var options = new DbContextOptionsBuilder<BatusSalesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new BatusSalesDbContext(options, context);
        db.Contacts.Add(new Contact
        {
            DisplayName = "Wrong tenant",
            OrganizationId = Guid.NewGuid()
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Unbound_tenant_cannot_write_even_when_record_has_no_tenant()
    {
        var options = new DbContextOptionsBuilder<BatusSalesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new BatusSalesDbContext(options, new TenantContext());
        db.Contacts.Add(new Contact { DisplayName = "Blocked" });

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
}
