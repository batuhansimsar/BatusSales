using BatusSales.Api.Auth;
using BatusSales.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Infrastructure;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    TenantContext tenantContext) : DbContext(options)
{
    private readonly Guid _tenantId = tenantContext.TenantId ?? Guid.Empty;

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("contacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt })
                .HasDatabaseName("ix_contacts_tenant_created_at");
            entity.HasQueryFilter(x => x.TenantId == _tenantId);
        });

        modelBuilder.Entity<TenantMembership>(entity =>
        {
            entity.ToTable("tenant_memberships");
            entity.HasKey(x => new { x.TenantId, x.SubjectId });
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.SubjectId).HasColumnName("subject_id").HasMaxLength(200);
            entity.HasQueryFilter(x => x.TenantId == _tenantId);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Defense-in-depth: no contact write can silently move data across tenants.
        foreach (var entry in ChangeTracker.Entries<Contact>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (_tenantId == Guid.Empty || entry.Entity.TenantId != _tenantId)
                throw new UnauthorizedAccessException("Cross-tenant contact write rejected.");

            if (entry.State != EntityState.Added &&
                entry.Property(x => x.TenantId).OriginalValue != _tenantId)
                throw new UnauthorizedAccessException("Cross-tenant contact write rejected.");
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
