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

    public EtlService(ProjectsDbContext projectsCtx, HrDbContext hrCtx, WarehouseDbContext dwhCtx)
    {
        _projectsCtx = projectsCtx;
        _hrCtx = hrCtx;
        _dwhCtx = dwhCtx;
    }

    public async Task RunIntegrationAsync()
    {
        Console.WriteLine("ETL: Извлечение данных из источников...");

        var source1Employees = await _projectsCtx.Employees
            .Include(e => e.Assignments)
            .ThenInclude(a => a.Project)
            .ToListAsync();

        var source2Staff = await _hrCtx.Staff
            .Include(s => s.Finances)
            .ToListAsync();

        // Очистка DWH перед заливкой
        _dwhCtx.FactProjectExperiences.RemoveRange(_dwhCtx.FactProjectExperiences);
        _dwhCtx.DimEmployees.RemoveRange(_dwhCtx.DimEmployees);
        await _dwhCtx.SaveChangesAsync();

        int matchedCount = 0;

        foreach (var staff in source2Staff)
        {
            string hrName = NormalizeFio(staff.PersonName);

            // Ищем совпадение. Теперь используем Fuzzy Matching (перекрытие имен)
            var matchingEmployee = source1Employees.FirstOrDefault(e =>
                IsSamePerson(hrName, NormalizeFio(e.FullName)));

            var dimEmployee = new DimEmployee
            {
                OriginalSourceId = staff.Id,
                UnifiedFullName = CapitalizeName(staff.PersonName),
                Resume = staff.ResumeText,
                TotalIncome = staff.Finances.Sum(f => f.Salary + f.Bonus)
            };

            _dwhCtx.DimEmployees.Add(dimEmployee);
            await _dwhCtx.SaveChangesAsync();

            if (matchingEmployee != null)
            {
                matchedCount++;
                foreach (var assignment in matchingEmployee.Assignments)
                {
                    _dwhCtx.FactProjectExperiences.Add(new FactProjectExperience
                    {
                        EmployeeKey = dimEmployee.EmployeeKey,
                        ProjectName = assignment.Project.Title,
                        Role = assignment.RoleName,
                        TechStack = ExtractTechStack(assignment.Project.Description)
                    });
                }
            }
        }

        await _dwhCtx.SaveChangesAsync();
        Console.WriteLine($"ETL завершен. Всего сотрудников: {source2Staff.Count}, Совпадений найдено: {matchedCount}");
    }

    /// <summary>
    /// Проверяет, являются ли строки одним и тем же человеком.
    /// Если в одной строке "Иванов Иван", а в другой "Иванов Иван Иванович", 
    /// то все слова из короткой строки должны присутствовать в длинной.
    /// </summary>
    private bool IsSamePerson(string name1, string name2)
    {
        if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2)) return false;

        var parts1 = name1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parts2 = name2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Проверяем, что все слова из более короткого варианта есть в более длинном
        var smaller = parts1.Length <= parts2.Length ? parts1 : parts2;
        var larger = parts1.Length > parts2.Length ? parts1 : parts2;

        return smaller.All(s => larger.Contains(s));
    }

    private string NormalizeFio(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName)) return string.Empty;

        // Убираем точки (для инициалов) и приводим к нижнему регистру
        string clean = Regex.Replace(rawName.ToLower(), @"[\.]", " ");

        return string.Join(" ", clean.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(w => w.Trim())
                                     .Where(w => w.Length > 1)); // Игнорируем одиночные буквы без точек
    }

    private string CapitalizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpper(s[0]) + s.Substring(1).ToLower()));
    }

    private string ExtractTechStack(string description)
    {
        if (string.IsNullOrEmpty(description)) return "Не указан";
        return description.Contains("Stack:") ? description.Split("Stack:").Last().Trim() : "Общий стек";
    }
}