
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models.SourceProjects;

public class Assignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmployeeId { get; set; }
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;

    public Guid ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public virtual Project Project { get; set; } = null!;

    public string RoleName { get; set; } = string.Empty; // Dev, QA, PM
}