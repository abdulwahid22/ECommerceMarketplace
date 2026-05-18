using IdentityService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.FirstName)
     .IsRequired()
     .HasMaxLength(100);

                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(30);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.EmailConfirmed)
                    .HasDefaultValue(false);

                entity.Property(u => u.PhoneNumberConfirmed)
                    .HasDefaultValue(false);

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                entity.Property(u => u.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                entity.Property(u => u.CreatedByUserId)
               .IsRequired(false);

                entity.Property(u => u.UpdatedAt)
                    .IsRequired(false);
                entity.Property(u => u.UpdatedByUserId)
               .IsRequired(false);

                entity.Property(u => u.DeletedAt)
                    .IsRequired(false);

                entity.Property(u => u.DeletedByUserId)
                    .IsRequired(false);

                entity.Property(u => u.LastLoginAt)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(r => r.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.Token)
                    .IsRequired();

                entity.HasIndex(rt => rt.Token)
                    .IsUnique();

                entity.Property(rt => rt.ExpiresAt)
                    .IsRequired();

                entity.Property(rt => rt.IsRevoked)
                    .HasDefaultValue(false);

                entity.Property(rt => rt.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId);
            });
            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.HasKey(evt => evt.Id);

                entity.Property(evt => evt.Token)
                    .IsRequired();

                entity.HasIndex(evt => evt.Token)
                    .IsUnique();

                entity.Property(evt => evt.ExpiresAt)
                    .IsRequired();

                entity.Property(evt => evt.IsUsed)
                    .HasDefaultValue(false);

                entity.Property(evt => evt.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(evt => evt.User)
                    .WithMany(u => u.EmailVerificationTokens)
                    .HasForeignKey(evt => evt.UserId);
            });
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(prt => prt.Id);

                entity.Property(prt => prt.Token)
                    .IsRequired();

                entity.HasIndex(prt => prt.Token)
                    .IsUnique();

                entity.Property(prt => prt.ExpiresAt)
                    .IsRequired();

                entity.Property(prt => prt.IsUsed)
                    .HasDefaultValue(false);

                entity.Property(prt => prt.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(prt => prt.User)
                    .WithMany(u => u.PasswordResetTokens)
                    .HasForeignKey(prt => prt.UserId);
            });
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(al => al.Id);

                entity.Property(al => al.Action)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(al => al.EntityName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(al => al.OldValues)
                    .IsRequired(false);

                entity.Property(al => al.NewValues)
                    .IsRequired(false);

                entity.Property(al => al.Description)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(al => al.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasIndex(al => al.PerformedByUserId);

                entity.HasIndex(al => al.EntityName);

                entity.HasIndex(al => al.EntityId);

                entity.HasIndex(al => al.CreatedAt);
            });
        }
    }
}