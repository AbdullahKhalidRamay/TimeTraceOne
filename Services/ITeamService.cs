using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface ITeamService
{
    Task<PaginatedResponse<TeamDto>> GetTeamsAsync(TeamFilterDto filter);
    Task<TeamDto?> GetTeamByIdAsync(Guid id);
    Task<TeamDto> CreateTeamAsync(CreateTeamDto dto, Guid createdBy);
    Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamDto dto);
    Task DeleteTeamAsync(Guid id);
    
    // Team Member Management
    Task<TeamDto> AddMemberToTeamAsync(Guid teamId, AddTeamMemberDto dto);
    Task<TeamDto> RemoveMemberFromTeamAsync(Guid teamId, Guid userId);
    Task<TeamDto> UpdateTeamLeaderAsync(Guid teamId, UpdateTeamLeaderDto dto);
    
    // Team Associations
    Task<TeamDto> AssociateTeamWithProjectAsync(Guid teamId, AssociateTeamProjectDto dto);
    Task<TeamDto> AssociateTeamWithProductAsync(Guid teamId, AssociateTeamProductDto dto);
    Task<TeamDto> AssociateTeamWithDepartmentAsync(Guid teamId, AssociateTeamDepartmentDto dto);
    
    // User Teams
    Task<List<TeamSummaryDto>> GetUserTeamsAsync(Guid userId);
}
