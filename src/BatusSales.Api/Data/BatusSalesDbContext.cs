using BatusSales.Api.Domain;
using BatusSales.Api.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Data;

/// <summary>
/// Defense-in-depth tenant filters and write guards.
/// Filters are NOT authorization by themselves: authenticate and verify
/// organization membership before ever initializing the tenant context.
/// </summary>
public sealed class BatusSalesDbContext(
    DbContextOptions<BatusSalesDbContext> options,
    ITenantContext tenantContext) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> Messages => Set<ConversationMessage>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.Phone).HasMaxLength(32);
            entity.HasIndex(x => new { x.OrganizationId, x.Email });
            entity.HasQueryFilter(x => tenantContext.HasTenant && x.OrganizationId == tenantContext.OrganizationId);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Channel).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.OrganizationId, x.ContactId });
            entity.HasQueryFilter(x => tenantContext.HasTenant && x.OrganizationId == tenantContext.OrganizationId);
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Body).HasMaxLength(10000).IsRequired();
            entity.HasIndex(x => new { x.OrganizationId, x.ConversationId, x.CreatedAtUtc });
            entity.HasQueryFilter(x => tenantContext.HasTenant && x.OrganizationId == tenantContext.OrganizationId);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.OrganizationId, x.StaffId, x.StartsAtUtc });
            entity.HasQueryFilter(x => tenantContext.HasTenant && x.OrganizationId == tenantContext.OrganizationId);
        });
    }

    public override int SaveChanges()
    {
        EnforceTenantWrites();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceTenantWrites();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnforceTenantWrites()
    {
        var entries = ChangeTracker.Entries<TenantEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        foreach (var entry in entries)
        {
            if (!tenantContext.HasTenant)
                throw new InvalidOperationException("Tenant context is required for tenant data writes.");

            if (entry.State == EntityState.Added && entry.Entity.OrganizationId == Guid.Empty)
                entry.Entity.OrganizationId = tenantContext.OrganizationId;

            if (entry.Entity.OrganizationId != tenantContext.OrganizationId)
                throw new InvalidOperationException("Cross-tenant data write blocked.");

            if (entry.State != EntityState.Added &&
                entry.Property(nameof(TenantEntity.OrganizationId)).IsModified)
                throw new InvalidOperationException("Changing a record's tenant is not supported.");
        }
    }
}
