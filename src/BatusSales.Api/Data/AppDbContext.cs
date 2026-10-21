using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    // Set only after a server-side membership check; unset contexts cannot see contacts.
    public Guid? ActiveTenantId { get; private set; }
    internal void ActivateTenant(Guid organizationId) => ActiveTenantId = organizationId;
    internal void ClearTenant() => ActiveTenantId = null;

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMembership> Memberships => Set<OrganizationMembership>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<KnowledgeEntry> KnowledgeEntries => Set<KnowledgeEntry>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organization>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(150);
        });

        builder.Entity<OrganizationMembership>(entity =>
        {
            entity.HasKey(x => new { x.OrganizationId, x.UserId });
            entity.HasIndex(x => x.UserId);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<AppUser>().WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Contact>(entity =>
        {
            entity.HasIndex(x => new { x.OrganizationId, x.CreatedAtUtc });
            // EF includes the original organization ID in UPDATE/DELETE predicates.
            // This prevents detached entities with guessed IDs from crossing tenants.
            entity.Property(x => x.OrganizationId).IsConcurrencyToken();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Phone).HasMaxLength(40);
            // Context-property parameterization; no authorized organization means zero rows.
            entity.HasQueryFilter(x => !x.IsDeleted && x.OrganizationId == ActiveTenantId);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<KnowledgeEntry>(entity =>
        {
            entity.HasIndex(x => new { x.OrganizationId, x.CreatedAtUtc });
            entity.Property(x => x.OrganizationId).IsConcurrencyToken();
            entity.Property(x => x.Question).IsRequired().HasMaxLength(300);
            entity.Property(x => x.Answer).IsRequired().HasMaxLength(4000);
            entity.HasQueryFilter(x => !x.IsDeleted && x.OrganizationId == ActiveTenantId);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AuditEvent>(entity =>
        {
            // Internal audit reads fail closed until exact database membership
            // activates the current tenant. IgnoreQueryFilters() is privileged.
            entity.HasQueryFilter(x => x.OrganizationId == ActiveTenantId);
            entity.HasIndex(x => new { x.OrganizationId, x.CreatedAtUtc });
            entity.Property(x => x.Action).IsRequired().HasMaxLength(80);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateTenantOwnedMutations();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantOwnedMutations();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateTenantOwnedMutations()
    {
        ChangeTracker.DetectChanges();
        foreach (var entry in ChangeTracker.Entries<Contact>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (ActiveTenantId is null || entry.Entity.OrganizationId != ActiveTenantId.Value)
                throw new UnauthorizedAccessException(
                    "Contact writes require verified membership in the matching organization.");

            if (entry.State == EntityState.Modified &&
                entry.Property(x => x.OrganizationId).IsModified)
                throw new UnauthorizedAccessException("Contacts cannot move between organizations.");
        }
        foreach (var entry in ChangeTracker.Entries<KnowledgeEntry>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;
            if (ActiveTenantId is null || entry.Entity.OrganizationId != ActiveTenantId.Value)
                throw new UnauthorizedAccessException(
                    "FAQ writes require verified membership in the matching organization.");
            if (entry.State == EntityState.Modified &&
                entry.Property(x => x.OrganizationId).IsModified)
                throw new UnauthorizedAccessException("FAQ entries cannot move between organizations.");
        }
    }
}
