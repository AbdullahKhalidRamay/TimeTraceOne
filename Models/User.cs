using System.ComponentModel.DataAnnotations;

namespace TimeTraceOne.Models;

public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.employee;
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [Range(0, 24)]
    public decimal AvailableHours { get; set; } = 8.0m;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public virtual ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public virtual ICollection<Team> CreatedTeams { get; set; } = new List<Team>();
    public virtual ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    public virtual ICollection<Product> CreatedProducts { get; set; } = new List<Product>();
    public virtual ICollection<Department> CreatedDepartments { get; set; } = new List<Department>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<ApprovalHistory> ApprovalHistory { get; set; } = new List<ApprovalHistory>();
}
