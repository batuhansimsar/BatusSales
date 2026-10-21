using BatuSales.Api.Domain;
using BatuSales.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace BatuSales.Api.Data;

/// <summary>
/// Contact reads are filtered by the verified tenant scope. All contact writes
/// are checked too; filters alone do NOT protect writes or raw SQL.
/// </summary>
public sealed class BatuSalesDbContext(
    DbContextOptions<BatuSalesDbContext> options,
    TenantScope tenantScope) : DbContext(options)
{
    public Guid? ActiveOrganizationId => tenantScope.OrganizationId;

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> Members => Set<OrganizationMember>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(e =>
        {
            e.ToTable("organizations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<OrganizationMember>(e =>
        {
            e.ToTable("organization_members");
            e.HasKey(x => new { x.OrganizationId, x.UserId });
            e.Property(x => x.OrganizationId).HasColumnName("organization_id");
            e.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(255);
            e.Property(x => x.Role).HasColumnName("role").HasMaxLength(16).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Contact>(e =>
        {
            e.ToTable("contacts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.OrganizationId).HasColumnName("organization_id");
            e.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(160).IsRequired();
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(254);
            e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(32);
            e.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(2000);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne<Organization>().WithMany()
                .HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.OrganizationId, x.CreatedAt })
                .HasDatabaseName("idx_contacts_org_created").IsDescending(false, true);

            // EF parameterizes the CURRENT request's verified organization ID.
            e.HasQueryFilter(x => ActiveOrganizationId.HasValue
                && x.OrganizationId == ActiveOrganizationId.Value);
        });
    }

    public override int SaveChanges()
    {
        GuardContactWrites();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GuardContactWrites();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void GuardContactWrites()
    {
        foreach (var entry in ChangeTracker.Entries<Contact>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (!tenantScope.OrganizationId.HasValue
                || entry.Entity.OrganizationId != tenantScope.OrganizationId.Value)
                throw new InvalidOperationException("Cross-tenant contact write blocked.");

            if (entry.State == EntityState.Modified && entry.Property(x => x.OrganizationId).IsModified)
                throw new InvalidOperationException("Changing a contact's organization is forbidden.");
        }
    }
}
