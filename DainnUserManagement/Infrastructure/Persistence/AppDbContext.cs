using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using DainnUserManagement.Domain.Entities;
using System.Linq;

namespace DainnUserManagement.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public DbSet<Permission> Permissions { get; set; } = null!;

    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Get provider-specific SQL for current timestamp
        // This ensures compatibility with all database providers
        string currentTimestampSql = GetCurrentTimestampSql();

        // Configure all Guid properties to use UUID type in PostgreSQL
        if (Database.IsNpgsql())
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(Guid) || p.ClrType == typeof(Guid?)))
                {
                    property.SetColumnType("uuid");
                    
                    // Set default value for primary key Guid properties (non-nullable)
                    if (property.ClrType == typeof(Guid) && !property.IsNullable && 
                        entityType.FindPrimaryKey()?.Properties.Contains(property) == true)
                    {
                        property.SetDefaultValueSql("gen_random_uuid()");
                    }
                }
            }
        }

        // Configure AppUser
        builder.Entity<AppUser>(entity =>
        {
            // Configure default values
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(currentTimestampSql);

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql(currentTimestampSql);

            // Email index (unique - already handled by Identity)
        });

        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            // Primary key default value for PostgreSQL
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
            }

            // Token index
            entity.HasIndex(e => e.Token)
                .IsUnique();

            // CreatedAt default value
            entity.Property(e => e.Created)
                .HasDefaultValueSql(currentTimestampSql);
        });

        // Configure AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            // Primary key default value for PostgreSQL
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
            }

            // Timestamp default value
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql(currentTimestampSql);
        });

        // Configure Permission
        builder.Entity<Permission>(entity =>
        {
            // Primary key default value for PostgreSQL
            if (Database.IsNpgsql())
            {
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("gen_random_uuid()");
            }

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(currentTimestampSql);

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql(currentTimestampSql);
        });

        // Configure AppRole
        builder.Entity<AppRole>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql(currentTimestampSql);

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql(currentTimestampSql);
        });

        // Configure RolePermission composite key
        builder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Configure relationships
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.Roles)
            .HasForeignKey(rp => rp.PermissionId);
    }

    /// <summary>
    /// Gets the provider-specific SQL function for current timestamp.
    /// This ensures compatibility with all supported database providers.
    /// </summary>
    private string GetCurrentTimestampSql()
    {
        // EF Core providers handle CURRENT_TIMESTAMP differently:
        // - PostgreSQL, SQLite: CURRENT_TIMESTAMP (SQL standard)
        // - SQL Server: GETDATE() (preferred, but CURRENT_TIMESTAMP also works)
        // - MySQL: CURRENT_TIMESTAMP (SQL standard)
        // 
        // For maximum compatibility, we use provider-specific functions where needed
        if (Database.IsSqlServer())
        {
            return "GETDATE()";
        }

        // For PostgreSQL, SQLite, MySQL, and others, use SQL standard CURRENT_TIMESTAMP
        return "CURRENT_TIMESTAMP";
    }
}

