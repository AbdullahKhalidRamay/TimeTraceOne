using System.ComponentModel.DataAnnotations;
using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class CreateTeamDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid DepartmentId { get; set; }
    
    public Guid? LeaderId { get; set; }
    
    public List<Guid>? MemberIds { get; set; } = new List<Guid>();
    public List<Guid>? ProjectIds { get; set; } = new List<Guid>();
    public List<Guid>? ProductIds { get; set; } = new List<Guid>();
}

public class UpdateTeamDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public Guid? DepartmentId { get; set; }
    
    public Guid? LeaderId { get; set; }
}

public class TeamDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid? LeaderId { get; set; }
    public string? LeaderName { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public List<Guid> MemberIds { get; set; } = new List<Guid>();
    public List<string> MemberNames { get; set; } = new List<string>();
    public List<Guid> AssociatedProjects { get; set; } = new List<Guid>();
    public List<string> ProjectNames { get; set; } = new List<string>();
    public List<Guid> AssociatedProducts { get; set; } = new List<Guid>();
    public List<string> ProductNames { get; set; } = new List<string>();
    public List<Guid> AssociatedDepartments { get; set; } = new List<Guid>();
    public List<string> DepartmentNames { get; set; } = new List<string>();
}

public class TeamFilterDto
{
    public Guid? DepartmentId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class TeamSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public Guid? LeaderId { get; set; }
    public string? LeaderName { get; set; }
    public int MemberCount { get; set; }
}

public class AddTeamMemberDto
{
    [Required]
    public Guid UserId { get; set; }
}

public class UpdateTeamLeaderDto
{
    [Required]
    public Guid LeaderId { get; set; }
}

public class AssociateTeamProjectDto
{
    [Required]
    public Guid ProjectId { get; set; }
}

public class AssociateTeamProductDto
{
    [Required]
    public Guid ProductId { get; set; }
}

public class AssociateTeamDepartmentDto
{
    [Required]
    public Guid DepartmentId { get; set; }
}

public class TeamMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}
