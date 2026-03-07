
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models.Warehouse;

public class DimEmployee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EmployeeKey { get; set; } // Суррогатный ключ (Identity)

    public Guid OriginalSourceId { get; set; } // Для прослеживаемости (Lineage)
    public string UnifiedFullName { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalIncome { get; set; }

    public virtual ICollection<FactProjectExperience> Experiences { get; set; } = [];
}
