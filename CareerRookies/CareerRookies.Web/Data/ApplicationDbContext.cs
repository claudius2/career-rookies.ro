using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Models;
using CareerRookies.Web.Models.Interfaces;

namespace CareerRookies.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<WorkshopRegistration> WorkshopRegistrations => Set<WorkshopRegistration>();
    public DbSet<WorkshopMedia> WorkshopMedia => Set<WorkshopMedia>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<CareerResource> CareerResources => Set<CareerResource>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override int SaveChanges()
    {
        ApplyConventions();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyConventions();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyConventions()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<ITimestamped>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity key column lengths
        builder.Entity<IdentityUser>(entity =>
        {
            entity.Property(u => u.Id).HasMaxLength(450);
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.NormalizedEmail).HasMaxLength(256);
            entity.Property(u => u.UserName).HasMaxLength(256);
            entity.Property(u => u.NormalizedUserName).HasMaxLength(256);
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.Property(r => r.Id).HasMaxLength(450);
            entity.Property(r => r.Name).HasMaxLength(256);
            entity.Property(r => r.NormalizedName).HasMaxLength(256);
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(l => l.LoginProvider).HasMaxLength(128);
            entity.Property(l => l.ProviderKey).HasMaxLength(128);
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(t => t.LoginProvider).HasMaxLength(128);
            entity.Property(t => t.Name).HasMaxLength(128);
        });

        // Workshop
        builder.Entity<Workshop>(entity =>
        {
            entity.HasMany(w => w.Registrations)
                .WithOne(r => r.Workshop)
                .HasForeignKey(r => r.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(w => w.Media)
                .WithOne(m => m.Workshop)
                .HasForeignKey(m => m.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(w => w.Date);
            entity.HasIndex(w => w.Slug).IsUnique();
            entity.HasIndex(w => w.IsDeleted);
            entity.HasQueryFilter(w => !w.IsDeleted);
        });

        // WorkshopRegistration
        builder.Entity<WorkshopRegistration>(entity =>
        {
            entity.HasOne(r => r.StudentClass)
                .WithMany()
                .HasForeignKey(r => r.StudentClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new { r.WorkshopId, r.StudentName, r.StudentClassId });
        });

        // Article
        builder.Entity<Article>(entity =>
        {
            entity.HasIndex(a => a.Status);
            entity.HasIndex(a => a.Slug).IsUnique();
            entity.HasIndex(a => a.IsDeleted);
            entity.HasQueryFilter(a => !a.IsDeleted);

            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // Testimonial
        builder.Entity<Testimonial>(entity =>
        {
            entity.HasIndex(t => t.IsApproved);
            entity.HasIndex(t => t.IsDeleted);
            entity.HasQueryFilter(t => !t.IsDeleted);

            entity.Property(t => t.AuthorType)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // CareerResource
        builder.Entity<CareerResource>(entity =>
        {
            entity.Property(r => r.Category)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // SiteSetting
        builder.Entity<SiteSetting>(entity =>
        {
            entity.HasIndex(s => s.Key).IsUnique();
        });

        // WorkshopMedia
        builder.Entity<WorkshopMedia>(entity =>
        {
            entity.Property(m => m.MediaType)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
            entity.HasIndex(a => a.Timestamp);
        });
    }
}
