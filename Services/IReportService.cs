using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface IReportService
{
    // User Reports
    Task<UserReportDto> GetUserReportAsync(Guid userId, string startDate, string endDate);
    Task<UserWeeklyReportDto> GetUserWeeklyReportAsync(Guid userId, string weekStart);
    Task<UserMonthlyReportDto> GetUserMonthlyReportAsync(Guid userId, string month);
    Task<List<UserReportDto>> GetUsersReportAsync(string startDate, string endDate);
    
    // Team Reports
    Task<TeamReportDto> GetTeamReportAsync(Guid teamId, string startDate, string endDate);
    Task<TeamReportDto> GetTeamWeeklyReportAsync(Guid teamId, string weekStart);
    Task<List<TeamReportDto>> GetTeamsReportAsync(string startDate, string endDate);
    
    // System Reports
    Task<SystemOverviewDto> GetSystemOverviewAsync(string startDate, string endDate);
    Task<List<DepartmentPerformanceDto>> GetDepartmentPerformanceAsync(string startDate, string endDate);
    Task<DepartmentPerformanceDto> GetDepartmentPerformanceByIdAsync(Guid departmentId, string startDate, string endDate);
    Task<List<ProjectPerformanceReportDto>> GetProjectPerformanceAsync(string startDate, string endDate, string? status);
    Task<ProjectPerformanceReportDto> GetProjectPerformanceByIdAsync(Guid projectId, string startDate, string endDate);
    
    // Export Reports
    Task<byte[]> ExportCsvReportAsync(ExportRequestDto request);
    Task<byte[]> ExportPdfReportAsync(ExportRequestDto request);
    
    // Search and Analytics
    Task<TimeEntrySearchResultDto> SearchTimeEntriesAsync(string query, string startDate, string endDate);
    Task<List<ProjectPerformanceReportDto>> GetTopProjectsForDepartmentAsync(Guid departmentId, string startDate, string endDate);
}
