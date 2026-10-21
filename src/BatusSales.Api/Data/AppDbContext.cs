using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMembership> Memberships => Set<OrganizationMembership>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    // Activated only by TenantAccess after verifying the caller's database membership.
    // A missing/denied membership must never default to a tenant.
    public Guid? AuthorizedTenantId { get; private set; }

    internal void ClearAuthorizedTenant() => AuthorizedTenantId = null;

    internal void SetAuthorizedTenant(Guid organizationId)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization ID cannot be empty.", nameof(organizationId));
        AuthorizedTenantId = organizationId;
    }

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
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Phone).HasMaxLength(40);
            // EF parameterizes the DbContext property: one compiled model can serve
            // different authorized organizations without caching a tenant ID.
            // Keep soft-delete and tenant rules together; query filters are
            // defense-in-depth, not a substitute for endpoint authorization.
            entity.HasQueryFilter(x => !x.IsDeleted && x.OrganizationId == AuthorizedTenantId);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AuditEvent>(entity =>
        {
            entity.HasIndex(x => new { x.OrganizationId, x.CreatedAtUtc });
            entity.Property(x => x.Action).IsRequired().HasMaxLength(80);
            entity.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    // Cover both synchronous and async SaveChanges overloads. A tenant filter
    // only governs reads; it cannot prevent tracked cross-tenant writes.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateContactMutations();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ValidateContactMutations();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateContactMutations()
    {
        foreach (var entry in ChangeTracker.Entries<Contact>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (AuthorizedTenantId is not Guid tenantId ||
                entry.Entity.OrganizationId != tenantId ||
                (entry.State == EntityState.Modified &&
                 entry.Property(x => x.OrganizationId).IsModified))
            {
                throw new UnauthorizedAccessException(
                    "Contact writes require verified tenant membership and a matching, immutable organization.");
            }
        }
    }

}
