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
            entity.HasQueryFilter(x => !x.IsDeleted);
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
}
