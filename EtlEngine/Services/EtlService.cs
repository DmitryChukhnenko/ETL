using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Data.Contexts;
using Data.Models.Warehouse;

namespace EtlEngine.Services;

public class EtlService
{
    private readonly ProjectsDbContext _projectsCtx;
    private readonly HrDbContext _hrCtx;
    private readonly WarehouseDbContext _dwhCtx;

    public EtlService(
        ProjectsDbContext projectsCtx,
        HrDbContext hrCtx,
        WarehouseDbContext dwhCtx)
    {
        _projectsCtx = projectsCtx;
        _hrCtx = hrCtx;
        _dwhCtx = dwhCtx;
    }

    public async Task RunIntegrationAsync()
    {
        Console.WriteLine("Начало этапа Extract...");

        // Извлекаем данные из первого источника (Проекты)
        var source1Employees = await _projectsCtx.Employees
            .Include(e => e.Assignments)
            .ThenInclude(a => a.Project)
            .ToListAsync();

        // Извлекаем данные из второго источника (HR)
        var source2Staff = await _hrCtx.Staff
            .Include(s => s.Finances)
            .ToListAsync();

        Console.WriteLine("Начало этапа Transform & Load...");

        // Очищаем целевые таблицы перед загрузкой (опционально для тестов)
        _dwhCtx.FactProjectExperiences.RemoveRange(_dwhCtx.FactProjectExperiences);
        _dwhCtx.DimEmployees.RemoveRange(_dwhCtx.DimEmployees);
        await _dwhCtx.SaveChangesAsync();

        foreach (var staff in source2Staff)
        {
            // 1. Нормализация имени из HR базы для поиска
            string normalizedHrName = NormalizeFio(staff.PersonName);

            // 2. Поиск соответствия в базе проектов
            var matchingEmployee = source1Employees.FirstOrDefault(e =>
                NormalizeFio(e.FullName) == normalizedHrName);

            // 3. Создание записи в измерении сотрудников (DimEmployee)
            var dimEmployee = new DimEmployee
            {
                OriginalSourceId = staff.Id,
                UnifiedFullName = CapitalizeName(normalizedHrName), // Делаем красиво: "Иванов Иван Иванович"
                Resume = staff.ResumeText,
                TotalIncome = staff.Finances.Sum(f => f.Salary + f.Bonus)
            };

            _dwhCtx.DimEmployees.Add(dimEmployee);
            await _dwhCtx.SaveChangesAsync(); // Сохраняем, чтобы получить Identity ID (EmployeeKey)

            // 4. Если нашли совпадение по проектам, переносим опыт в таблицу фактов
            if (matchingEmployee != null)
            {
                foreach (var assignment in matchingEmployee.Assignments)
                {
                    var factExperience = new FactProjectExperience
                    {
                        EmployeeKey = dimEmployee.EmployeeKey,
                        ProjectName = assignment.Project.Title,
                        Role = assignment.RoleName,
                        TechStack = ExtractTechStack(assignment.Project.Description)
                    };
                    _dwhCtx.FactProjectExperiences.Add(factExperience);
                }
            }
        }

        await _dwhCtx.SaveChangesAsync();
        Console.WriteLine("Процесс ETL успешно завершен.");
    }

    /// <summary>
    /// Ключевой метод трансформации: нормализует ФИО.
    /// Убирает точки, лишние пробелы, приводит к нижнему регистру и сортирует слова.
    /// Это позволяет сопоставить "Иванов Иван" и "Иван И. Иванов".
    /// </summary>
    private string NormalizeFio(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName)) return string.Empty;

        // Убираем точки и спецсимволы, переводим в нижний регистр
        string clean = Regex.Replace(rawName.ToLower(), @"[\.\,]", " ");

        // Разбиваем на слова, убираем короткие части (инициалы без точек)
        var words = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                         .Select(w => w.Trim())
                         .Where(w => w.Length > 1)
                         .OrderBy(w => w) // Сортировка важна для сравнения при разном порядке слов
                         .ToList();

        return string.Join(" ", words);
    }

    /// <summary>
    /// Приводит нормализованное имя к формату заголовка (Иван Иванов).
    /// </summary>
    private string CapitalizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return string.Join(" ", name.Split(' ').Select(s =>
            char.ToUpper(s[0]) + s.Substring(1)));
    }

    /// <summary>
    /// Простая логика извлечения стека из описания проекта.
    /// </summary>
    private string ExtractTechStack(string description)
    {
        if (description.Contains("Stack:"))
        {
            return description.Split("Stack:").Last().Trim();
        }
        return "Не указан";
    }
}