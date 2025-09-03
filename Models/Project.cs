using System.ComponentModel.DataAnnotations;

namespace TimeTraceOne.Models;

public class Project : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    public ProjectType ProjectType { get; set; }
    
    [MaxLength(100)]
    public string? ClientName { get; set; }
    
    [MaxLength(255)]
    [EmailAddress]
    public string? ClientEmail { get; set; }
    
    public bool IsBillable { get; set; }
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    
    [Required]
    public Guid CreatedBy { get; set; }
    
    // Navigation properties
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<TeamProject> TeamProjects { get; set; } = new List<TeamProject>();
}
