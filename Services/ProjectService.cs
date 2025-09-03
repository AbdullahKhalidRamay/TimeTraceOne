using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class ProjectService : IProjectService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<ProjectService> _logger;
    
    public ProjectService(TimeFlowDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<ProjectDto>> GetProjectsAsync(ProjectFilterDto filter)
    {
        var query = _context.Projects
            .Include(p => p.Creator)
            .Include(p => p.TeamProjects)
                .ThenInclude(tp => tp.Team)
            .Include(p => p.TeamProjects)
                .ThenInclude(tp => tp.Team.Department)
            .AsQueryable();
            
        // Apply filters
        if (filter.IsBillable.HasValue)
            query = query.Where(p => p.IsBillable == filter.IsBillable.Value);
            
        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status.Value);
            
        if (filter.DepartmentId.HasValue)
            query = query.Where(p => p.TeamProjects.Any(tp => tp.Team.DepartmentId == filter.DepartmentId.Value));
            
        if (filter.TeamId.HasValue)
            query = query.Where(p => p.TeamProjects.Any(tp => tp.TeamId == filter.TeamId.Value));
            
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search) || 
                                   (p.Description != null && p.Description.Contains(filter.Search)));
            
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / filter.Limit);
        
        var projects = await query
            .OrderBy(p => p.Name)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        var projectDtos = projects.Select(MapToProjectDto).ToList();
        
        return PaginatedResponse<ProjectDto>.CreateSuccess(projectDtos, filter.Page, filter.Limit, total, totalPages);
    }
    
    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.Creator)
            .Include(p => p.TeamProjects)
                .ThenInclude(tp => tp.Team)
            .Include(p => p.TeamProjects)
                .ThenInclude(tp => tp.Team.Department)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        return project != null ? MapToProjectDto(project) : null;
    }
    
    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid createdBy)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            ProjectType = dto.ProjectType,
            ClientName = dto.ClientName,
            ClientEmail = dto.ClientEmail,
            IsBillable = dto.IsBillable,
            Status = ProjectStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Projects.Add(project);
        
        // Add team associations if specified
        if (dto.TeamIds?.Any() == true)
        {
            foreach (var teamId in dto.TeamIds)
            {
                var teamProject = new TeamProject
                {
                    TeamId = teamId,
                    ProjectId = project.Id
                };
                _context.TeamProjects.Add(teamProject);
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created project {ProjectId} with name {ProjectName}", project.Id, project.Name);
        
        return await GetProjectByIdAsync(project.Id) ?? throw new InvalidOperationException("Failed to retrieve created project");
    }
    
    public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto)
    {
        var project = await _context.Projects
            .Include(p => p.TeamProjects)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (project == null)
            throw new InvalidOperationException("Project not found");
            
        if (dto.Name != null)
            project.Name = dto.Name;
            
        if (dto.Description != null)
            project.Description = dto.Description;
            
        if (dto.ProjectType.HasValue)
            project.ProjectType = dto.ProjectType.Value;
            
        if (dto.ClientName != null)
            project.ClientName = dto.ClientName;
            
        if (dto.ClientEmail != null)
            project.ClientEmail = dto.ClientEmail;
            
        if (dto.IsBillable.HasValue)
            project.IsBillable = dto.IsBillable.Value;
            
        if (dto.Status.HasValue)
            project.Status = dto.Status.Value;
            
        project.UpdatedAt = DateTime.UtcNow;
        
        // Update team associations if specified
        if (dto.TeamIds != null)
        {
            // Remove existing associations
            var existingAssociations = project.TeamProjects.ToList();
            foreach (var association in existingAssociations)
            {
                _context.TeamProjects.Remove(association);
            }
            
            // Add new associations
            foreach (var teamId in dto.TeamIds)
            {
                var teamProject = new TeamProject
                {
                    TeamId = teamId,
                    ProjectId = project.Id
                };
                _context.TeamProjects.Add(teamProject);
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated project {ProjectId}", id);
        
        return await GetProjectByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated project");
    }
    
    public async Task DeleteProjectAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
            throw new InvalidOperationException("Project not found");
            
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted project {ProjectId}", id);
    }
    
    public async Task<List<ProjectSummaryDto>> GetUserAssociatedProjectsAsync(Guid userId)
    {
        var projects = await _context.Projects
            .Where(p => p.CreatedBy == userId || 
                       p.TeamProjects.Any(tp => tp.Team.Members.Any(m => m.UserId == userId)))
            .Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                IsBillable = p.IsBillable,
                Status = p.Status
            })
            .ToListAsync();
            
        return projects;
    }
    
    private static ProjectDto MapToProjectDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            ProjectType = project.ProjectType,
            ClientName = project.ClientName,
            ClientEmail = project.ClientEmail,
            IsBillable = project.IsBillable,
            Status = project.Status,
            CreatedBy = project.CreatedBy,
            CreatorName = project.Creator?.Name ?? string.Empty,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            DepartmentIds = project.TeamProjects.Select(tp => tp.Team.DepartmentId).Distinct().ToList(),
            TeamIds = project.TeamProjects.Select(tp => tp.TeamId).ToList(),
            DepartmentNames = project.TeamProjects.Select(tp => tp.Team.Department.Name).Distinct().ToList(),
            TeamNames = project.TeamProjects.Select(tp => tp.Team.Name).ToList()
        };
    }
    
    public async Task<ProjectStatisticsDto> GetProjectStatisticsAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
            throw new InvalidOperationException("Project not found");
            
        var timeEntries = await _context.TimeEntries
            .Where(t => t.ProjectDetails.Contains(id.ToString()))
            .ToListAsync();
            
        var totalHours = timeEntries.Sum(t => t.ActualHours);
        var totalBillableHours = timeEntries.Sum(t => t.BillableHours);
        var workingDays = timeEntries.Select(t => t.Date.Date).Distinct().Count();
        
        return new ProjectStatisticsDto
        {
            ProjectId = id,
            ProjectName = project.Name,
            TotalHours = totalHours,
            TotalBillableHours = totalBillableHours,
            WorkingDays = workingDays,
            AverageHoursPerDay = workingDays > 0 ? totalHours / workingDays : 0,
            TotalEntries = timeEntries.Count
        };
    }
    
    public async Task<List<TeamMemberDto>> GetProjectTeamMembersAsync(Guid id)
    {
        var teamMembers = await _context.TeamProjects
            .Where(tp => tp.ProjectId == id)
            .SelectMany(tp => tp.Team.Members)
            .Select(m => new TeamMemberDto
            {
                UserId = m.UserId,
                UserName = m.User.Name,
                TeamId = m.TeamId,
                TeamName = m.Team.Name,
                JoinedAt = m.JoinedAt
            })
            .ToListAsync();
            
        return teamMembers;
    }
    
    public async Task AddTeamToProjectAsync(Guid projectId, Guid teamId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
            throw new InvalidOperationException("Project not found");
            
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
            throw new InvalidOperationException("Team not found");
            
        var existingAssociation = await _context.TeamProjects
            .FirstOrDefaultAsync(tp => tp.ProjectId == projectId && tp.TeamId == teamId);
            
        if (existingAssociation != null)
            throw new InvalidOperationException("Team is already associated with this project");
            
        var teamProject = new TeamProject
        {
            TeamId = teamId,
            ProjectId = projectId
        };
        
        _context.TeamProjects.Add(teamProject);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added team {TeamId} to project {ProjectId}", teamId, projectId);
    }
    
    public async Task RemoveTeamFromProjectAsync(Guid projectId, Guid teamId)
    {
        var association = await _context.TeamProjects
            .FirstOrDefaultAsync(tp => tp.ProjectId == projectId && tp.TeamId == teamId);
            
        if (association == null)
            throw new InvalidOperationException("Team is not associated with this project");
            
        _context.TeamProjects.Remove(association);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Removed team {TeamId} from project {ProjectId}", teamId, projectId);
    }
}
