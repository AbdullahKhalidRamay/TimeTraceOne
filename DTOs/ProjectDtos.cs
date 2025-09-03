using System.ComponentModel.DataAnnotations;
using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class CreateProjectDto
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
    
    public bool IsBillable { get; set; } = true;
    
    public List<Guid>? DepartmentIds { get; set; } = new List<Guid>();
    public List<Guid>? TeamIds { get; set; } = new List<Guid>();
}

public class UpdateProjectDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public ProjectType? ProjectType { get; set; }
    
    [MaxLength(100)]
    public string? ClientName { get; set; }
    
    [MaxLength(255)]
    [EmailAddress]
    public string? ClientEmail { get; set; }
    
    public bool? IsBillable { get; set; }
    
    public ProjectStatus? Status { get; set; }
    
    public List<Guid>? DepartmentIds { get; set; }
    public List<Guid>? TeamIds { get; set; }
}

public class ProjectDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectType ProjectType { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }
    public bool IsBillable { get; set; }
    public ProjectStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public List<Guid> DepartmentIds { get; set; } = new List<Guid>();
    public List<Guid> TeamIds { get; set; } = new List<Guid>();
    public List<string> DepartmentNames { get; set; } = new List<string>();
    public List<string> TeamNames { get; set; } = new List<string>();
}

public class ProjectFilterDto
{
    public bool? IsBillable { get; set; }
    public ProjectStatus? Status { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class ProjectSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public ProjectStatus Status { get; set; }
}

public class ProjectStatisticsDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public int WorkingDays { get; set; }
    public decimal AverageHoursPerDay { get; set; }
    public int TotalEntries { get; set; }
}

public class AddTeamToProjectDto
{
    public Guid TeamId { get; set; }
}
