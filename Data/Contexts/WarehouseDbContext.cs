
using Data.Models.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    public DbSet<DimEmployee> DimEmployees { get; set; }
    public DbSet<FactProjectExperience> FactProjectExperiences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FactProjectExperience>()
            .HasOne(f => f.Employee)
            .WithMany(e => e.Experiences)
            .HasForeignKey(f => f.EmployeeKey);
    }
}
