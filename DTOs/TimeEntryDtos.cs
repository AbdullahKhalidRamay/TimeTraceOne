using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class ProjectDetailsDto
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateTimeEntryDto
{
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [Range(0, 24)]
    public decimal ActualHours { get; set; }
    
    [Required]
    [Range(0, 24)]
    public decimal BillableHours { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Task { get; set; } = string.Empty;
    
    [Required]
    public ProjectDetailsDto ProjectDetails { get; set; } = null!;
    
    public bool IsBillable { get; set; }
}

public class UpdateTimeEntryDto
{
    [Range(0, 24)]
    public decimal? ActualHours { get; set; }
    
    [Range(0, 24)]
    public decimal? BillableHours { get; set; }
    
    [MaxLength(1000)]
    public string? Task { get; set; }
    
    public ProjectDetailsDto? ProjectDetails { get; set; }
    
    public bool? IsBillable { get; set; }
}

public class TimeEntryDto : BaseDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal ActualHours { get; set; }
    public decimal BillableHours { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AvailableHours { get; set; }
    public string Task { get; set; } = string.Empty;
    public ProjectDetailsDto ProjectDetails { get; set; } = null!;
    public bool IsBillable { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntryStatus Status { get; set; }
}

public class TimeEntryFilterDto
{
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List<EntryStatus>? Status { get; set; }
    public bool? IsBillable { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class WeeklyBulkRequestDto
{
    [Required]
    public string WeekStart { get; set; } = string.Empty; // YYYY-MM-DD
    
    [Required]
    public List<WeeklyEntryDto> Entries { get; set; } = new List<WeeklyEntryDto>();
}

public class WeeklyEntryDto
{
    public string? ProjectId { get; set; }
    public string? ProductId { get; set; }
    public string? DepartmentId { get; set; }
    public Dictionary<string, DailyHoursDto> DailyHours { get; set; } = new Dictionary<string, DailyHoursDto>();
}

public class DailyHoursDto
{
    [Range(0, 24)]
    public decimal ActualHours { get; set; }
    
    [Range(0, 24)]
    public decimal BillableHours { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Task { get; set; } = string.Empty;
}

public class WeeklyBulkResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int CreatedEntries { get; set; }
    public int SkippedEntries { get; set; }
    public string WeekStart { get; set; } = string.Empty;
    public string WeekEnd { get; set; } = string.Empty;
}

public class WeeklyTimeEntriesDto
{
    public string WeekStart { get; set; } = string.Empty;
    public string WeekEnd { get; set; } = string.Empty;
    public List<DailyEntriesDto> Entries { get; set; } = new List<DailyEntriesDto>();
    public WeeklySummaryDto WeeklySummary { get; set; } = null!;
}

public class DailyEntriesDto
{
    public DateTime Date { get; set; }
    public List<TimeEntryDto> Entries { get; set; } = new List<TimeEntryDto>();
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
}

public class WeeklySummaryDto
{
    public decimal TotalActualHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public int TotalEntries { get; set; }
    public int PendingEntries { get; set; }
    public int ApprovedEntries { get; set; }
}

public class UpdateWeeklyTimeEntriesDto
{
    [Required]
    public List<UpdateWeeklyEntryDto> Entries { get; set; } = new List<UpdateWeeklyEntryDto>();
}

public class UpdateWeeklyEntryDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Range(0, 24)]
    public decimal? ActualHours { get; set; }
    
    [Range(0, 24)]
    public decimal? BillableHours { get; set; }
    
    [MaxLength(1000)]
    public string? Task { get; set; }
}
