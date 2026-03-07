
using Data.Models.SourceProjects;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Data.Contexts;

public class ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Assignment> Assignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Assignments)
            .HasForeignKey(a => a.EmployeeId);

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Project)
            .WithMany(p => p.Assignments)
            .HasForeignKey(a => a.ProjectId);
    }
}
