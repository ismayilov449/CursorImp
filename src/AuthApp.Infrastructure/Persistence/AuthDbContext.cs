using AuthApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Infrastructure.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(up => new { up.UserId, up.PermissionId });
            entity.HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId);
            entity.HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => new { rt.TokenHash, rt.TokenSalt }).IsUnique();
            entity.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(512);
            entity.Property(rt => rt.TokenSalt).IsRequired().HasMaxLength(128);
            entity.Property(rt => rt.CreatedByIp).HasMaxLength(64);
            entity.Property(rt => rt.RevokedByIp).HasMaxLength(64);
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);
        });
    }
}
