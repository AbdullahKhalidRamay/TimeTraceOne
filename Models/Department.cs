using System.ComponentModel.DataAnnotations;

namespace TimeTraceOne.Models;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? DepartmentDescription { get; set; }
    
    public bool IsBillable { get; set; }
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    
    [Required]
    public Guid CreatedBy { get; set; }
    
    // Navigation properties
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    public virtual ICollection<TeamDepartment> TeamDepartments { get; set; } = new List<TeamDepartment>();
}
