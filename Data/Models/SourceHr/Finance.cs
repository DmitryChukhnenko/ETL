using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models.SourceHr;

public class Finance
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StaffId { get; set; }
    [ForeignKey("StaffId")]
    public virtual Staff Staff { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; }
}
