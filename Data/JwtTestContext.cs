using JwtTest.Models.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtTest.Data;

public partial class JwtTestContext : DbContext, IDataProtectionKeyContext
{
    public JwtTestContext()
    {
    }

    public JwtTestContext(DbContextOptions<JwtTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Attendance> Attendances { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07CF82A7CC");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61600A9315D3").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RoleName).HasMaxLength(100);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                            .UsingEntity<Dictionary<string, object>>(
                                "RolePermission",
                                r => r.HasOne<Permission>().WithMany()
                                    .HasForeignKey("PermissionId")
                                    .HasConstraintName("FK_RolePermissions_Permissions"),
                                l => l.HasOne<Role>().WithMany()
                                    .HasForeignKey("RoleId")
                                    .HasConstraintName("FK_RolePermissions_Roles"),
                                j =>
                                {
                                    j.HasKey("RoleId", "PermissionId");
                                    j.ToTable("RolePermissions");
                                });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07774CBC3C");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053420B58537").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_UserRoles_Roles"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_UserRoles_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(rt => rt.Id).ValueGeneratedOnAdd();
            entity.HasIndex(rt => rt.Token)
                  .IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        });
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.PermissionName)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(60);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public bool Equals(JwtTestContext other)
    {
        throw new NotImplementedException();
    }
}
