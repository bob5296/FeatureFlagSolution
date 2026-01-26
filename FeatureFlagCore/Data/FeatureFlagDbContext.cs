using FeatureFlagCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlagCore.Data;

public class FeatureFlagDbContext : DbContext
{
    public FeatureFlagDbContext(DbContextOptions<FeatureFlagDbContext> options) : base(options)
    {
    }
    
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<UserOverride> UserOverrides => Set<UserOverride>();
    public DbSet<GroupOverride> GroupOverrides => Set<GroupOverride>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => f.Key).IsUnique();
            entity.Property(f => f.Key).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Description).HasMaxLength(500);
        });
        
        modelBuilder.Entity<UserOverride>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => new { u.FeatureFlagId, u.UserId }).IsUnique();
            entity.Property(u => u.UserId).IsRequired().HasMaxLength(100);
            
            entity.HasOne(u => u.FeatureFlag)
                  .WithMany(f => f.UserOverrides)
                  .HasForeignKey(u => u.FeatureFlagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<GroupOverride>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.HasIndex(g => new { g.FeatureFlagId, g.GroupId }).IsUnique();
            entity.Property(g => g.GroupId).IsRequired().HasMaxLength(100);
            
            entity.HasOne(g => g.FeatureFlag)
                  .WithMany(f => f.GroupOverrides)
                  .HasForeignKey(g => g.FeatureFlagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
