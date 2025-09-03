using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class ReportPeriodDto
{
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}

public class UserReportDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ReportPeriodDto Period { get; set; } = new ReportPeriodDto();
    public UserStatisticsDto Stats { get; set; } = new UserStatisticsDto();
    public List<DailyBreakdownDto> DailyBreakdown { get; set; } = new List<DailyBreakdownDto>();
}

public class TeamReportDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public ReportPeriodDto Period { get; set; } = new ReportPeriodDto();
    public TeamStatisticsDto TeamStats { get; set; } = new TeamStatisticsDto();
    public List<TeamMemberBreakdownDto> MemberBreakdown { get; set; } = new List<TeamMemberBreakdownDto>();
    public List<ProjectBreakdownDto> ProjectBreakdown { get; set; } = new List<ProjectBreakdownDto>();
}

public class TeamStatisticsDto
{
    public int TotalMembers { get; set; }
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal AverageHoursPerMember { get; set; }
    public int TotalEntries { get; set; }
}

public class TeamMemberBreakdownDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public int EntriesCount { get; set; }
}

public class ProjectBreakdownDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
}

public class SystemOverviewDto
{
    public ReportPeriodDto Period { get; set; } = new ReportPeriodDto();
    public SystemStatisticsDto SystemStats { get; set; } = new SystemStatisticsDto();
    public UserActivityDto UserActivity { get; set; } = new UserActivityDto();
    public ProjectPerformanceDto ProjectPerformance { get; set; } = new ProjectPerformanceDto();
}

public class SystemStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalProjects { get; set; }
    public int TotalProducts { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalTeams { get; set; }
    public int TotalTimeEntries { get; set; }
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal ApprovalRate { get; set; }
    public decimal BillableRate { get; set; }
}

public class UserActivityDto
{
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
}

public class ProjectPerformanceDto
{
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public decimal AverageProjectHours { get; set; }
}

public class DepartmentPerformanceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal AverageHoursPerMember { get; set; }
    public decimal BillableRate { get; set; }
    public List<ProjectBreakdownDto> TopProjects { get; set; } = new List<ProjectBreakdownDto>();
    
    // Additional properties for specific department reports
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public int TotalProjects { get; set; }
    public int TeamCount { get; set; }
}

public class ProjectPerformanceReportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal BillableRate { get; set; }
    public int TeamCount { get; set; }
    public int MemberCount { get; set; }
    public decimal AverageHoursPerMember { get; set; }
    public decimal CompletionPercentage { get; set; }
}

public class ExportRequestDto
{
    public string ReportType { get; set; } = string.Empty; // user, team, system, project
    public Guid? UserId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string Format { get; set; } = "csv"; // csv, pdf
}

public class TimeEntrySearchDto
{
    public string Query { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}

public class TimeEntrySearchResultDto
{
    public string Query { get; set; } = string.Empty;
    public List<TimeEntrySearchItemDto> Results { get; set; } = new List<TimeEntrySearchItemDto>();
    public int TotalResults { get; set; }
}

public class TimeEntrySearchItemDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public ProjectDetailsDto ProjectDetails { get; set; } = new ProjectDetailsDto();
    public string Date { get; set; } = string.Empty;
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public EntryStatus Status { get; set; }
}

public class TimeEntryAdvancedFilterDto
{
    public Guid? UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? DepartmentId { get; set; }
    public List<EntryStatus>? Status { get; set; }
    public bool? IsBillable { get; set; }
    public decimal? MinHours { get; set; }
    public decimal? MaxHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SortBy { get; set; } // date, hours, status
    public string? SortOrder { get; set; } // asc, desc
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class TimeEntryAdvancedFilterResultDto
{
    public TimeEntryAdvancedFilterDto Filters { get; set; } = new TimeEntryAdvancedFilterDto();
    public List<TimeEntryDto> Results { get; set; } = new List<TimeEntryDto>();
    public int TotalResults { get; set; }
    public FilterSummaryDto Summary { get; set; } = new FilterSummaryDto();
}

public class TimeEntryFilterResultDto
{
    public TimeEntryAdvancedFilterDto Filters { get; set; } = new TimeEntryAdvancedFilterDto();
    public List<TimeEntryFilterItemDto> Results { get; set; } = new List<TimeEntryFilterItemDto>();
    public int TotalResults { get; set; }
    public TimeEntryFilterSummaryDto Summary { get; set; } = new TimeEntryFilterSummaryDto();
}

public class TimeEntryFilterItemDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public EntryStatus Status { get; set; }
}

public class TimeEntryFilterSummaryDto
{
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
}

public class FilterSummaryDto
{
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
}

public class TimeEntryStatusDto
{
    public string Date { get; set; } = string.Empty;
    public TimeEntryStatusInfoDto Status { get; set; } = new TimeEntryStatusInfoDto();
}

public class TimeEntryStatusInfoDto
{
    public bool HasEntries { get; set; }
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public int EntriesCount { get; set; }
    public List<EntryStatus> Statuses { get; set; } = new List<EntryStatus>();
    public List<TimeEntryStatusEntryDto> Entries { get; set; } = new List<TimeEntryStatusEntryDto>();
}

public class TimeEntryStatusEntryDto
{
    public Guid Id { get; set; }
    public ProjectDetailsDto ProjectDetails { get; set; } = new ProjectDetailsDto();
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public EntryStatus Status { get; set; }
}

public class TimeEntryStatusItemDto
{
    public Guid Id { get; set; }
    public ProjectDetailsDto ProjectDetails { get; set; } = new ProjectDetailsDto();
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public EntryStatus Status { get; set; }
}

public class TimeEntryValidationDto
{
    public string Date { get; set; } = string.Empty;
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public Guid UserId { get; set; }
}

public class TimeEntryValidationResultDto
{
    public bool IsValid { get; set; }
    public ValidationRulesDto ValidationRules { get; set; } = new ValidationRulesDto();
    public List<string> Errors { get; set; } = new List<string>();
}

public class ValidationRulesDto
{
    public int MaxDailyHours { get; set; } = 24;
    public int MaxWeeklyHours { get; set; } = 168;
    public bool BillableHoursValid { get; set; }
    public bool NoOverlap { get; set; }
}
