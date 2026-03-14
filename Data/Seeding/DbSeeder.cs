using Data.Contexts;
using Data.Models.SourceHr;
using Data.Models.SourceProjects;

namespace Data.Seeding;

public static class DbSeeder
{
    public static async Task SeedAsync(ProjectsDbContext projectsCtx, HrDbContext hrCtx)
    {
        // Проверка, что базы пусты
        if (projectsCtx.Employees.Any() || hrCtx.Staff.Any()) return;

        // 1. Заполнение базы Проектов (Source 1)
        var emp1 = new Employee { FullName = "Иванов Иван" };
        var emp2 = new Employee { FullName = "ПЕТРОВ П.П." };

        var project = new Project
        {
            Title = "E-Commerce Platform",
            Description = "Stack: .NET 10, React, PostgreSQL"
        };

        projectsCtx.Employees.AddRange(emp1, emp2);
        projectsCtx.Projects.Add(project);

        projectsCtx.Assignments.Add(new Assignment
        {
            Employee = emp1,
            Project = project,
            RoleName = "Backend Developer"
        });

        // 2. Заполнение базы HR (Source 2)
        var staff1 = new Staff
        {
            PersonName = "иванов иван иванович",
            ResumeText = "Опытный разработчик на C#"
        };
        var staff2 = new Staff
        {
            PersonName = "Петров Петр Петрович",
            ResumeText = "QA Engineer с опытом автоматизации"
        };

        hrCtx.Staff.AddRange(staff1, staff2);

        hrCtx.Finances.Add(new Finance { Staff = staff1, Salary = 150000, Bonus = 20000 });
        hrCtx.Finances.Add(new Finance { Staff = staff2, Salary = 120000, Bonus = 10000 });

        await projectsCtx.SaveChangesAsync();
        await hrCtx.SaveChangesAsync();

        Console.WriteLine("Тестовые данные успешно созданы в Source 1 и Source 2.");
    }
}