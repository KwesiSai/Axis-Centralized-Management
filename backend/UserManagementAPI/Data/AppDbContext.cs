using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Models;

namespace UserManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Models.Application> Applications => Set<Models.Application>();
    public DbSet<UserApplication> UserApplications => Set<UserApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraint on username
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Unique constraint: a user can only be assigned to an app once
        modelBuilder.Entity<UserApplication>()
            .HasIndex(ua => new { ua.UserId, ua.ApplicationId })
            .IsUnique();

        // Seed the two required applications
        modelBuilder.Entity<Models.Application>().HasData(
            new Models.Application { Id = 1, Name = "HR", Description = "Human Resources application" },
            new Models.Application { Id = 2, Name = "Accounting", Description = "Accounting and finance application" }
        );
    }
}
