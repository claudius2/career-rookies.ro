using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CareerRookies.Web.Models;

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

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Workshop workshop)
                workshop.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Article article)
                article.UpdatedAt = DateTime.UtcNow;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Fix Identity key column lengths for SQL Server indexing
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
        });

        builder.Entity<WorkshopRegistration>(entity =>
        {
            entity.HasOne(r => r.StudentClass)
                .WithMany()
                .HasForeignKey(r => r.StudentClassId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SiteSetting>(entity =>
        {
            entity.HasIndex(s => s.Key).IsUnique();
        });

        builder.Entity<WorkshopMedia>(entity =>
        {
            entity.Property(m => m.MediaType)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        builder.Entity<Testimonial>(entity =>
        {
            entity.Property(t => t.AuthorType)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        builder.Entity<CareerResource>(entity =>
        {
            entity.Property(r => r.Category)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        builder.Entity<Article>(entity =>
        {
            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
        });
    }
}
