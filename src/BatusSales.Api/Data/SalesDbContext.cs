using BatusSales.Api.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BatusSales.Api.Data;

public sealed class SalesDbContext(
    DbContextOptions<SalesDbContext> options,
    CurrentTenant tenant) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Contact> Contacts => Set<Contact>();

    // EF parameterizes the context property rather than freezing a tenant ID in the model.
    public Guid? ActiveTenantId => tenant.Id;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organization>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        });

        builder.Entity<Membership>(b =>
        {
            b.HasKey(x => new { x.OrganizationId, x.UserId });
            b.Property(x => x.Role).HasMaxLength(32).IsRequired();
            b.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Contact>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(160).IsRequired();
            b.Property(x => x.Email).HasMaxLength(254);
            b.Property(x => x.Phone).HasMaxLength(32);
            b.HasIndex(x => new { x.OrganizationId, x.CreatedAt });
            b.HasIndex(x => new { x.OrganizationId, x.Email });
            b.HasOne<Organization>()
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Fail closed when the request has not passed tenant membership authorization.
            b.HasQueryFilter(x =>
                ActiveTenantId.HasValue && x.OrganizationId == ActiveTenantId.Value);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Contact>())
        {
            if (entry.State is not (
                EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            if (!ActiveTenantId.HasValue || entry.Entity.OrganizationId != ActiveTenantId.Value)
            {
                throw new UnauthorizedAccessException(
                    "Contact mutations require a validated matching tenant.");
            }

            if (entry.State == EntityState.Modified &&
                entry.Property(x => x.OrganizationId).IsModified)
            {
                throw new UnauthorizedAccessException(
                    "Moving a contact between tenants is forbidden.");
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
