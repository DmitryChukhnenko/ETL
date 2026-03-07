
using Data.Models.SourceHr;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class HrDbContext(DbContextOptions<HrDbContext> options) : DbContext(options)
{
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Finance> Finances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Finance>()
            .HasOne(f => f.Staff)
            .WithMany(s => s.Finances)
            .HasForeignKey(f => f.StaffId);
    }
}
