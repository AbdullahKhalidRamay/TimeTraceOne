using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; } = UserRole.employee;
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [Range(0, 24)]
    public decimal AvailableHours { get; set; } = 8.0m;
    
    public List<Guid>? DepartmentIds { get; set; } = new List<Guid>();
    public List<Guid>? TeamIds { get; set; } = new List<Guid>();
}

public class UpdateUserDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    
    [Range(0, 24)]
    public decimal? AvailableHours { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole? Role { get; set; }
    
    public bool? IsActive { get; set; }
    
    public List<Guid>? DepartmentIds { get; set; }
    public List<Guid>? TeamIds { get; set; }
}

public class UserListDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
    public string? JobTitle { get; set; }
    public decimal AvailableHours { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> DepartmentIds { get; set; } = new List<Guid>();
    public List<Guid> TeamIds { get; set; } = new List<Guid>();
}

public class UserDetailDto : UserListDto
{
    public List<string> DepartmentNames { get; set; } = new List<string>();
    public List<string> TeamNames { get; set; } = new List<string>();
}

public class UserFilterDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole? Role { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? TeamId { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class UserStatisticsDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalEntries { get; set; }
    public int ApprovedEntries { get; set; }
    public int PendingEntries { get; set; }
    public int RejectedEntries { get; set; }
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal BillableRate { get; set; }
}

public class UserWeeklyReportDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string WeekStart { get; set; } = string.Empty;
    public string WeekEnd { get; set; } = string.Empty;
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
    public decimal OvertimeHours { get; set; }
    public int EntriesCount { get; set; }
    public List<DailyBreakdownDto> DailyBreakdown { get; set; } = new List<DailyBreakdownDto>();
}

public class UserMonthlyReportDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
    public decimal OvertimeHours { get; set; }
    public int EntriesCount { get; set; }
    public int WorkingDays { get; set; }
    public List<WeeklyBreakdownDto> WeeklyBreakdown { get; set; } = new List<WeeklyBreakdownDto>();
}

public class DailyBreakdownDto
{
    public string Date { get; set; } = string.Empty;
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public int EntriesCount { get; set; }
}

public class WeeklyBreakdownDto
{
    public string WeekStart { get; set; } = string.Empty;
    public string WeekEnd { get; set; } = string.Empty;
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
}

public class UserAvailableHoursDto
{
    public Guid UserId { get; set; }
    public string Date { get; set; } = string.Empty;
    public decimal AvailableHours { get; set; }
    public decimal UsedHours { get; set; }
    public decimal RemainingHours { get; set; }
    public decimal OvertimeHours { get; set; }
}
