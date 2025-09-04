using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface ITimeEntryService
{
    Task<List<TimeEntryDto>> GetTimeEntriesAsync(TimeEntryFilterDto filter);
    Task<TimeEntryDto?> GetTimeEntryByIdAsync(Guid id);
    Task<TimeEntryDto> CreateTimeEntryAsync(CreateTimeEntryDto dto, Guid userId);
    Task<TimeEntryDto> UpdateTimeEntryAsync(Guid id, UpdateTimeEntryDto dto, Guid userId, string userRole);
    Task DeleteTimeEntryAsync(Guid id, Guid userId, string userRole);
    Task<WeeklyBulkResponseDto> CreateWeeklyBulkAsync(WeeklyBulkRequestDto dto, Guid userId);
    Task<WeeklyTimeEntriesDto> GetWeeklyTimeEntriesAsync(string weekStart, Guid? userId = null);
    Task<TimeEntryDto> ApproveTimeEntryAsync(Guid id, string? message, Guid approverId);
    Task<TimeEntryDto> RejectTimeEntryAsync(Guid id, string message, Guid rejectorId);
    Task<TimeEntryStatusDto> GetTimeEntryStatusAsync(string date, Guid? userId = null);
    Task<TimeEntrySearchResultDto> SearchTimeEntriesAsync(string query, Guid? userId = null, string? startDate = null, string? endDate = null);
    Task<TimeEntryFilterResultDto> FilterTimeEntriesAsync(TimeEntryAdvancedFilterDto filter);
    Task<WeeklyBulkResponseDto> UpdateWeeklyTimeEntriesAsync(string weekStart, UpdateWeeklyTimeEntriesDto dto, Guid userId);
    Task<List<TimeEntryDto>> GetTimeEntriesByDateAsync(string date, Guid? userId = null);
    Task<List<TimeEntryDto>> GetTimeEntriesByUserAsync(Guid userId, string? startDate = null, string? endDate = null);
    Task<List<TimeEntryDto>> GetTimeEntriesByProjectAsync(Guid projectId, string? startDate = null, string? endDate = null);
    Task<List<TimeEntryDto>> GetTimeEntriesByDateRangeAsync(string startDate, string endDate, Guid? userId = null, Guid? projectId = null);
}
