
using System.ComponentModel.DataAnnotations;

namespace Data.Models.SourceProjects;

public class Employee
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;

    public virtual ICollection<Assignment> Assignments { get; set; } = [];
}