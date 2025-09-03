using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface IUserService
{
    Task<PaginatedResponse<UserListDto>> GetUsersAsync(UserFilterDto filter);
    Task<UserDetailDto?> GetUserByIdAsync(Guid id);
    Task<UserDetailDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDetailDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task DeleteUserAsync(Guid id);
    Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userId, string? startDate, string? endDate);
    Task<UserWeeklyReportDto> GetUserWeeklyReportAsync(Guid userId, string weekStart);
    Task<UserMonthlyReportDto> GetUserMonthlyReportAsync(Guid userId, string month);
    Task<UserAvailableHoursDto> GetUserAvailableHoursAsync(Guid userId, string date);
    Task<List<ProjectSummaryDto>> GetUserAssociatedProjectsAsync(Guid userId);
}
