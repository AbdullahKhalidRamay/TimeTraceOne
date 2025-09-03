using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class TeamService : ITeamService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<TeamService> _logger;
    
    public TeamService(TimeFlowDbContext context, ILogger<TeamService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<TeamDto>> GetTeamsAsync(TeamFilterDto filter)
    {
        var query = _context.Teams
            .Include(t => t.Department)
            .Include(t => t.Leader)
            .Include(t => t.Creator)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.TeamProjects)
                .ThenInclude(tp => tp.Project)
            .Include(t => t.TeamProducts)
                .ThenInclude(tp => tp.Product)
            .Include(t => t.TeamDepartments)
                .ThenInclude(td => td.Department)
            .AsQueryable();
            
        // Apply filters
        if (filter.DepartmentId.HasValue)
            query = query.Where(t => t.DepartmentId == filter.DepartmentId.Value);
            
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(t => t.Name.Contains(filter.Search) || 
                                   (t.Description != null && t.Description.Contains(filter.Search)));
            
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / filter.Limit);
        
        var teams = await query
            .OrderBy(t => t.Name)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        var teamDtos = teams.Select(MapToTeamDto).ToList();
        
        return PaginatedResponse<TeamDto>.CreateSuccess(teamDtos, filter.Page, filter.Limit, total, totalPages);
    }
    
    public async Task<TeamDto?> GetTeamByIdAsync(Guid id)
    {
        var team = await _context.Teams
            .Include(t => t.Department)
            .Include(t => t.Leader)
            .Include(t => t.Creator)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.TeamProjects)
                .ThenInclude(tp => tp.Project)
            .Include(t => t.TeamProducts)
                .ThenInclude(tp => tp.Product)
            .Include(t => t.TeamDepartments)
                .ThenInclude(td => td.Department)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        return team != null ? MapToTeamDto(team) : null;
    }
    
    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto, Guid createdBy)
    {
        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            DepartmentId = dto.DepartmentId,
            LeaderId = dto.LeaderId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Teams.Add(team);
        
        // Add members if specified
        if (dto.MemberIds?.Any() == true)
        {
            foreach (var memberId in dto.MemberIds)
            {
                var teamMember = new TeamMember
                {
                    TeamId = team.Id,
                    UserId = memberId,
                    JoinedAt = DateTime.UtcNow
                };
                _context.TeamMembers.Add(teamMember);
            }
        }
        
        // Add project associations if specified
        if (dto.ProjectIds?.Any() == true)
        {
            foreach (var projectId in dto.ProjectIds)
            {
                var teamProject = new TeamProject
                {
                    TeamId = team.Id,
                    ProjectId = projectId
                };
                _context.TeamProjects.Add(teamProject);
            }
        }
        
        // Add product associations if specified
        if (dto.ProductIds?.Any() == true)
        {
            foreach (var productId in dto.ProductIds)
            {
                var teamProduct = new TeamProduct
                {
                    TeamId = team.Id,
                    ProductId = productId
                };
                _context.TeamProducts.Add(teamProduct);
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created team {TeamId} with name {TeamName}", team.Id, team.Name);
        
        return await GetTeamByIdAsync(team.Id) ?? throw new InvalidOperationException("Failed to retrieve created team");
    }
    
    public async Task<TeamDto> UpdateTeamAsync(Guid id, UpdateTeamDto dto)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        if (dto.Name != null)
            team.Name = dto.Name;
            
        if (dto.Description != null)
            team.Description = dto.Description;
            
        if (dto.DepartmentId.HasValue)
            team.DepartmentId = dto.DepartmentId.Value;
            
        if (dto.LeaderId.HasValue)
            team.LeaderId = dto.LeaderId.Value;
            
        team.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated team {TeamId}", id);
        
        return await GetTeamByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated team");
    }
    
    public async Task DeleteTeamAsync(Guid id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted team {TeamId}", id);
    }
    
    public async Task<TeamDto> AddMemberToTeamAsync(Guid teamId, AddTeamMemberDto dto)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);
            
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        if (team.Members.Any(m => m.UserId == dto.UserId))
            throw new InvalidOperationException("User is already a member of this team");
            
        var teamMember = new TeamMember
        {
            TeamId = teamId,
            UserId = dto.UserId,
            JoinedAt = DateTime.UtcNow
        };
        
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added user {UserId} to team {TeamId}", dto.UserId, teamId);
        
        return await GetTeamByIdAsync(teamId) ?? throw new InvalidOperationException("Failed to retrieve updated team");
    }
    
    public async Task<TeamDto> RemoveMemberFromTeamAsync(Guid teamId, Guid userId)
    {
        var teamMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
            
        if (teamMember == null)
            throw new InvalidOperationException("User is not a member of this team");
            
        _context.TeamMembers.Remove(teamMember);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Removed user {UserId} from team {TeamId}", userId, teamId);
        
        return await GetTeamByIdAsync(teamId) ?? throw new InvalidOperationException("Failed to retrieve updated team");
    }
    
    public async Task<TeamDto> UpdateTeamLeaderAsync(Guid teamId, UpdateTeamLeaderDto dto)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        team.LeaderId = dto.LeaderId;
        team.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated team leader for team {TeamId} to {LeaderId}", teamId, dto.LeaderId);
        
        return await GetTeamByIdAsync(teamId) ?? throw new InvalidOperationException("Failed to retrieve updated team");
    }
    
    public async Task<TeamDto> AssociateTeamWithProjectAsync(Guid teamId, AssociateTeamProjectDto dto)
    {
        var team = await _context.Teams
            .Include(t => t.TeamProjects)
            .FirstOrDefaultAsync(t => t.Id == teamId);
            
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        if (team.TeamProjects.Any(tp => tp.ProjectId == dto.ProjectId))
            throw new InvalidOperationException("Team is already associated with this project");
            
        var teamProject = new TeamProject
        {
            TeamId = teamId,
            ProjectId = dto.ProjectId
        };
        
        _context.TeamProjects.Add(teamProject);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Associated team {TeamId} with project {ProjectId}", teamId, dto.ProjectId);
        
        return await GetTeamByIdAsync(teamId) ?? throw new InvalidOperationException("Failed to retrieve updated team");
    }
    
    public async Task<TeamDto> AssociateTeamWithProductAsync(Guid teamId, AssociateTeamProductDto dto)
    {
        var team = await _context.Teams
            .Include(t => t.TeamProducts)
            .FirstOrDefaultAsync(t => t.Id == teamId);
            
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        if (team.TeamProducts.Any(tp => tp.ProductId == dto.ProductId))
            throw new InvalidOperationException("Team is already associated with this product");
            
        var teamProduct = new TeamProduct
        {
            TeamId = teamId,
            ProductId = dto.ProductId
        };
        
        _context.TeamProducts.Add(teamProduct);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Associated team {TeamId} with product {ProductId}", teamId, dto.ProductId);
        
        return await GetTeamByIdAsync(teamId) ?? throw new InvalidOperationException("Failed to retrieve updated team");
    }
    
    public async Task<TeamDto> AssociateTeamWithDepartmentAsync(Guid teamId, AssociateTeamDepartmentDto dto)
    {
        var team = await _context.Teams
            .Include(t => t.TeamDepartments)
            .FirstOrDefaultAsync(t => t.Id == teamId);
            
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        if (team.TeamDepartments.Any(td => td.DepartmentId == dto.DepartmentId))
            throw new InvalidOperationException("Team is already associated with this department");
            
        var teamDepartment = new TeamDepartment
        {
            TeamId = teamId,
            DepartmentId = dto.DepartmentId
        };
        
        _context.TeamDepartments.Add(teamDepartment);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Associated team {TeamId} with department {DepartmentId}", teamId, dto.DepartmentId);
        
        return await GetTeamByIdAsync(teamId) ?? throw new InvalidOperationException("Failed to retrieve updated team");
    }
    
    public async Task<List<TeamSummaryDto>> GetUserTeamsAsync(Guid userId)
    {
        var teams = await _context.Teams
            .Include(t => t.Department)
            .Include(t => t.Leader)
            .Include(t => t.Members)
            .Where(t => t.Members.Any(m => m.UserId == userId))
            .Select(t => new TeamSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                DepartmentName = t.Department.Name,
                LeaderId = t.LeaderId,
                LeaderName = t.Leader != null ? t.Leader.Name : string.Empty,
                MemberCount = t.Members.Count
            })
            .ToListAsync();
            
        return teams;
    }
    
    private static TeamDto MapToTeamDto(Team team)
    {
        return new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            DepartmentId = team.DepartmentId,
            DepartmentName = team.Department?.Name ?? string.Empty,
            LeaderId = team.LeaderId,
            LeaderName = team.Leader?.Name ?? string.Empty,
            CreatedBy = team.CreatedBy,
            CreatorName = team.Creator?.Name ?? string.Empty,
            CreatedAt = team.CreatedAt,
            UpdatedAt = team.UpdatedAt,
            MemberIds = team.Members.Select(m => m.UserId).ToList(),
            MemberNames = team.Members.Select(m => m.User.Name).ToList(),
            AssociatedProjects = team.TeamProjects.Select(tp => tp.ProjectId).ToList(),
            ProjectNames = team.TeamProjects.Select(tp => tp.Project.Name).ToList(),
            AssociatedProducts = team.TeamProducts.Select(tp => tp.ProductId).ToList(),
            ProductNames = team.TeamProducts.Select(tp => tp.Product.Name).ToList(),
            AssociatedDepartments = team.TeamDepartments.Select(td => td.DepartmentId).ToList(),
            DepartmentNames = team.TeamDepartments.Select(td => td.Department.Name).ToList()
        };
    }
}
