using Microsoft.EntityFrameworkCore;
using ZenithPM.Web.Models.Entities;

namespace ZenithPM.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.MfaSecretKey).HasMaxLength(256);
            });

            modelBuilder.Entity<SecurityLog>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.ActionType).IsRequired().HasMaxLength(50);
                entity.Property(s => s.IpAddress).IsRequired().HasMaxLength(50);
                entity.Property(s => s.UserAgent).HasMaxLength(500);

                entity.HasOne(s => s.User)
                      .WithMany(u => u.SecurityLogs)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}