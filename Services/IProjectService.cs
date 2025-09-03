using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface IProjectService
{
    Task<PaginatedResponse<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy);
    Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto);
    Task DeleteProjectAsync(Guid id);
    Task<List<ProjectSummaryDto>> GetUserAssociatedProjectsAsync(Guid userId);
    Task<ProjectStatisticsDto> GetProjectStatisticsAsync(Guid id);
    Task<List<TeamMemberDto>> GetProjectTeamMembersAsync(Guid id);
    Task AddTeamToProjectAsync(Guid projectId, Guid teamId);
    Task RemoveTeamFromProjectAsync(Guid projectId, Guid teamId);
}
