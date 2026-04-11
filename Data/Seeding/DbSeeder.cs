using Data.Contexts;
using Data.Models.SourceHr;
using Data.Models.SourceProjects;

namespace Data.Seeding;

public static class DbSeeder
{

    /// <summary>
    /// Полная очистка всех трех баз данных
    /// </summary>
    public static async Task ClearAllDataAsync(ProjectsDbContext projectsCtx, HrDbContext hrCtx, WarehouseDbContext dwhCtx)
    {
        Console.WriteLine("Очистка существующих данных...");

        // Очищаем Хранилище (сначала факты, потом измерения из-за связей)
        dwhCtx.FactProjectExperiences.RemoveRange(dwhCtx.FactProjectExperiences);
        dwhCtx.DimEmployees.RemoveRange(dwhCtx.DimEmployees);

        // Очищаем Источник Проектов
        projectsCtx.Assignments.RemoveRange(projectsCtx.Assignments);
        projectsCtx.Projects.RemoveRange(projectsCtx.Projects);
        projectsCtx.Employees.RemoveRange(projectsCtx.Employees);

        // Очищаем Источник HR
        hrCtx.Finances.RemoveRange(hrCtx.Finances);
        hrCtx.Staff.RemoveRange(hrCtx.Staff);

        await dwhCtx.SaveChangesAsync();
        await projectsCtx.SaveChangesAsync();
        await hrCtx.SaveChangesAsync();

        Console.WriteLine("Базы данных очищены.");
    }


    public static async Task SeedAsync(ProjectsDbContext projectsCtx, HrDbContext hrCtx)
    {
        // Проверка, что базы пусты. Если нет - выходим.
        if (projectsCtx.Employees.Any() || hrCtx.Staff.Any()) return;

        // 1. Заполнение базы ПРОЕКТОВ (Source 1)
        var projects = new List<Project>
        {
            new Project { Title = "Cloud Migration", Description = "Stack: Azure, Terraform, Docker" },
            new Project { Title = "Mobile Banking", Description = "Stack: Kotlin, Swift, Firebase" },
            new Project { Title = "CRM Update", Description = "Stack: PHP, Vue.js, MySQL" }
        };
        projectsCtx.Projects.AddRange(projects);

        var pEmployees = new List<Employee>
        {
            new Employee { FullName = "Иванов Иван" }, // Совпадение 1
            new Employee { FullName = "Петров П.П." }, // Совпадение 2
            new Employee { FullName = "Сидоров Алексей" }, // Только в проектах
            new Employee { FullName = "Козлов Дмитрий" }, // Совпадение 3
            new Employee { FullName = "Смирнова Анна" }   // Совпадение 4
        };
        projectsCtx.Employees.AddRange(pEmployees);

        // Назначаем роли
        projectsCtx.Assignments.Add(new Assignment { Employee = pEmployees[0], Project = projects[0], RoleName = "DevOps" });
        projectsCtx.Assignments.Add(new Assignment { Employee = pEmployees[1], Project = projects[1], RoleName = "Mobile Lead" });
        projectsCtx.Assignments.Add(new Assignment { Employee = pEmployees[3], Project = projects[0], RoleName = "Backend" });
        projectsCtx.Assignments.Add(new Assignment { Employee = pEmployees[4], Project = projects[2], RoleName = "Frontend" });

        // 2. Заполнение базы HR (Source 2)
        var hrStaff = new List<Staff>
        {
            new Staff { PersonName = "Иванов Иван Иванович", ResumeText = "Эксперт по облачным технологиям" },
            new Staff { PersonName = "Петров Петр Петрович", ResumeText = "Разработчик мобильных приложений" },
            new Staff { PersonName = "Васильев Игорь", ResumeText = "Системный администратор" }, // Только в HR
            new Staff { PersonName = "Козлов Д. В.", ResumeText = "Backend разработчик" },
            new Staff { PersonName = "Смирнова Анна Сергеевна", ResumeText = "UI/UX дизайнер" }
        };
        hrCtx.Staff.AddRange(hrStaff);

        // Добавляем финансы
        hrCtx.Finances.Add(new Finance { Staff = hrStaff[0], Salary = 200000, Bonus = 50000 });
        hrCtx.Finances.Add(new Finance { Staff = hrStaff[1], Salary = 180000, Bonus = 30000 });
        hrCtx.Finances.Add(new Finance { Staff = hrStaff[2], Salary = 90000, Bonus = 5000 });
        hrCtx.Finances.Add(new Finance { Staff = hrStaff[3], Salary = 160000, Bonus = 20000 });
        hrCtx.Finances.Add(new Finance { Staff = hrStaff[4], Salary = 140000, Bonus = 15000 });

        await projectsCtx.SaveChangesAsync();
        await hrCtx.SaveChangesAsync();

        Console.WriteLine(">>> Базы данных успешно наполнены расширенным набором тестовых данных.");
    }
}