using System.ComponentModel.DataAnnotations;
using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class CreateDepartmentDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? DepartmentDescription { get; set; }
    
    public bool IsBillable { get; set; } = true;
}

public class UpdateDepartmentDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(2000)]
    public string? DepartmentDescription { get; set; }
    
    public bool? IsBillable { get; set; }
    
    public ProjectStatus? Status { get; set; }
}

public class DepartmentDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? DepartmentDescription { get; set; }
    public bool IsBillable { get; set; }
    public ProjectStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public int TeamCount { get; set; }
    public int MemberCount { get; set; }
}

public class DepartmentFilterDto
{
    public bool? IsBillable { get; set; }
    public ProjectStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class DepartmentSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public ProjectStatus Status { get; set; }
}
