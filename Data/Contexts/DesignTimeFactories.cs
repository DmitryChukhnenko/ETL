using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Data.Contexts;
using System.IO;

namespace Data.Design;

public abstract class BaseContextFactory
{
    protected IConfigurationRoot GetConfiguration()
    {
        // Указываем путь к папке проекта App, где лежит конфиг
        // Директория поднимется на уровень выше из Data в корень и зайдет в App
        var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "App");

        return new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile("appsettings.json")
            .Build();
    }
}

public class ProjectsContextFactory : BaseContextFactory, IDesignTimeDbContextFactory<ProjectsDbContext>
{
    public ProjectsDbContext CreateDbContext(string[] args)
    {
        var config = GetConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<ProjectsDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("ProjectsDb"));

        return new ProjectsDbContext(optionsBuilder.Options); 
    }
}

public class HrContextFactory : BaseContextFactory, IDesignTimeDbContextFactory<HrDbContext>
{
    public HrDbContext CreateDbContext(string[] args)
    {
        var config = GetConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<HrDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("HrDb"));

        return new HrDbContext(optionsBuilder.Options);
    }
}

public class WarehouseContextFactory : BaseContextFactory, IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public WarehouseDbContext CreateDbContext(string[] args)
    {
        var config = GetConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("WarehouseDb"));

        return new WarehouseDbContext(optionsBuilder.Options); 
    }
}