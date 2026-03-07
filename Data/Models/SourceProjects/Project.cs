
using System.ComponentModel.DataAnnotations;

namespace Data.Models.SourceProjects;

public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<Assignment> Assignments { get; set; } = [];
}