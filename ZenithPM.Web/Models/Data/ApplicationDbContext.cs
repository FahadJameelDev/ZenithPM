using Microsoft.EntityFrameworkCore;
// Hum ne 'Task' ko explicitly handle karne ke liye namespace alias use kiya hai
using TaskEntity = ZenithPM.Web.Models.Entities.Task;
using ZenithPM.Web.Models.Entities;

namespace ZenithPM.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Authentication & Security
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }

        // Enterprise Modules
        public DbSet<Project> Projects { get; set; }

        // Yahan humne alias 'TaskEntity' use kiya hai taake compiler confuse na ho
        public DbSet<TaskEntity> Tasks { get; set; }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Authentication & Security Entities ---

            // --- Enterprise Module Entities ---

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);

                // Added precision for Budget decimal property to resolve SQL warning/mapping
                entity.Property(p => p.Budget)
                      .HasPrecision(18, 2);

                entity.HasOne(p => p.Department).WithMany(d => d.Projects).HasForeignKey(p => p.DepartmentId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Manager).WithMany().HasForeignKey(p => p.ManagerId).OnDelete(DeleteBehavior.Restrict);
            });

            // Yahan TaskEntity use hoga
            modelBuilder.Entity<TaskEntity>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasOne(t => t.Project).WithMany(p => p.Tasks).HasForeignKey(t => t.ProjectId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(t => t.AssignedTo).WithMany().HasForeignKey(t => t.AssignedToId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.AssignedBy).WithMany().HasForeignKey(t => t.AssignedById).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.HasOne(d => d.Manager).WithMany().HasForeignKey(d => d.ManagerId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}