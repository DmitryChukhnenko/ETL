using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Data.Contexts;
using Data.Seeding;
using EtlEngine.Services;

namespace App;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("====================================================");
        Console.WriteLine("   Система ETL: Интеграция данных сотрудников      ");
        Console.WriteLine("====================================================");

        // 1. Настройка конфигурации из appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Получение строк подключения
        var projectsConn = configuration.GetConnectionString("ProjectsDb");
        var hrConn = configuration.GetConnectionString("HrDb");
        var warehouseConn = configuration.GetConnectionString("WarehouseDb");

        // 3. Настройка контекстов баз данных
        var projectsOptions = new DbContextOptionsBuilder<ProjectsDbContext>()
            .UseNpgsql(projectsConn)
            .Options;

        var hrOptions = new DbContextOptionsBuilder<HrDbContext>()
            .UseNpgsql(hrConn)
            .Options;

        var warehouseOptions = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseNpgsql(warehouseConn)
            .Options;

        // 4. Инициализация контекстов
        using var projectsCtx = new ProjectsDbContext(projectsOptions);
        using var hrCtx = new HrDbContext(hrOptions);
        using var dwhCtx = new WarehouseDbContext(warehouseOptions);

        try
        {
            // Шаг 1: Наполнение баз-источников тестовыми данными (Seeding)
            Console.WriteLine("\n[1/3] Наполнение источников тестовыми данными...");
            await DbSeeder.SeedAsync(projectsCtx, hrCtx);

            // Шаг 2: Запуск ETL сервиса
            Console.WriteLine("\n[2/3] Запуск процесса трансформации и загрузки (ETL)...");
            var etlService = new EtlService(projectsCtx, hrCtx, dwhCtx);
            await etlService.RunIntegrationAsync();

            // Шаг 3: Проверка результатов в Хранилище
            Console.WriteLine("\n[3/3] Проверка итоговых данных в Хранилище...");
            var employees = await dwhCtx.DimEmployees
                .Include(e => e.Experiences)
                .ToListAsync();

            Console.WriteLine($"\nВсего сотрудников в Хранилище: {employees.Count}");
            foreach (var emp in employees)
            {
                Console.WriteLine($"- {emp.UnifiedFullName} | Доход: {emp.TotalIncome:C} | Проектов: {emp.Experiences.Count}");
                foreach (var exp in emp.Experiences)
                {
                    Console.WriteLine($"  └ Проект: {exp.ProjectName} (Роль: {exp.Role}) | Стек: {exp.TechStack}");
                }
            }

            Console.WriteLine("\n====================================================");
            Console.WriteLine("   Работа программы успешно завершена!             ");
            Console.WriteLine("====================================================");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ОШИБКА]: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Подробности: {ex.InnerException.Message}");
            }
            Console.ResetColor();
        }
    }
}