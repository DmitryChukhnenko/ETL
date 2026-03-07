
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models.Warehouse;

public class FactProjectExperience
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int FactKey { get; set; }

    public int EmployeeKey { get; set; }
    [ForeignKey("EmployeeKey")]
    public virtual DimEmployee Employee { get; set; } = null!;

    public string ProjectName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TechStack { get; set; } = string.Empty;
}
