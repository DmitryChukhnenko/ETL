
using System.ComponentModel.DataAnnotations;

namespace Data.Models.SourceHr;

public class Staff
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PersonName { get; set; } = string.Empty;
    public string ResumeText { get; set; } = string.Empty;

    public virtual ICollection<Finance> Finances { get; set; } = [];
}
