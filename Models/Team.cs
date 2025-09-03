using System.ComponentModel.DataAnnotations;

namespace TimeTraceOne.Models;

public class Team : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid DepartmentId { get; set; }
    
    public Guid? LeaderId { get; set; }
    
    [Required]
    public Guid CreatedBy { get; set; }
    
    // Navigation properties
    public virtual Department Department { get; set; } = null!;
    public virtual User? Leader { get; set; }
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public virtual ICollection<TeamProject> TeamProjects { get; set; } = new List<TeamProject>();
    public virtual ICollection<TeamProduct> TeamProducts { get; set; } = new List<TeamProduct>();
    public virtual ICollection<TeamDepartment> TeamDepartments { get; set; } = new List<TeamDepartment>();
}
