using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface IValidationService
{
    Task<TimeEntryValidationResultDto> ValidateTimeEntryAsync(TimeEntryValidationDto dto);
    Task<UserAvailableHoursDto> GetUserAvailableHoursAsync(Guid userId, string date);
    Task<bool> ValidateUserAccessAsync(Guid userId, Guid resourceId, string resourceType);
    Task<bool> ValidateUserAccessAsync(Guid currentUserId, Guid userId);
    Task<bool> ValidateTeamAccessAsync(Guid userId, Guid teamId);
    Task<bool> ValidateProjectAccessAsync(Guid userId, Guid projectId);
}
