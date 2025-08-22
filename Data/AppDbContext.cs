using Microsoft.EntityFrameworkCore;
using TaskManagmentSystem.Entities;
using Task = TaskManagmentSystem.Entities.Task;

namespace TaskManagmentSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Task> Tasks { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(b =>
    {
        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.Phone_Number).IsUnique();
        b.HasOne(u => u.Department)
         .WithMany()
         .HasForeignKey(u => u.DepartmentID);
    });

            modelBuilder.Entity<Task>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserID);

            base.OnModelCreating(modelBuilder);
        }

    }
}
