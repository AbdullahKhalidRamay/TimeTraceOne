using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;
    
    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all projects with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProjectDto>>> GetProjects([FromQuery] ProjectFilterDto filter)
    {
        try
        {
            var projects = await _projectService.GetProjectsAsync(filter);
            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
    {
        try
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(ApiResponse<object>.Error("Project not found"));
                
            return Ok(ApiResponse<ProjectDto>.Success(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var currentUserId = GetCurrentUserId();
            var project = await _projectService.CreateProjectAsync(dto, currentUserId);
            
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, ApiResponse<ProjectDto>.Success(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Update project
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var project = await _projectService.UpdateProjectAsync(id, dto);
            return Ok(ApiResponse<ProjectDto>.Success(project));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Delete project
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> DeleteProject(Guid id)
    {
        try
        {
            await _projectService.DeleteProjectAsync(id);
            return Ok(ApiResponse<string>.Success("Project deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user associated projects
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<ProjectSummaryDto>>> GetUserProjects(Guid userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own projects unless they're Owner/Manager
            if (userId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var projects = await _projectService.GetUserAssociatedProjectsAsync(userId);
            return Ok(ApiResponse<List<ProjectSummaryDto>>.Success(projects));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user projects for {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get project statistics
    /// </summary>
    [HttpGet("{id:guid}/statistics")]
    public async Task<ActionResult<ProjectStatisticsDto>> GetProjectStatistics(Guid id)
    {
        try
        {
            var statistics = await _projectService.GetProjectStatisticsAsync(id);
            return Ok(ApiResponse<ProjectStatisticsDto>.Success(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project statistics for {ProjectId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get project team members
    /// </summary>
    [HttpGet("{id:guid}/team")]
    public async Task<ActionResult<List<TeamMemberDto>>> GetProjectTeam(Guid id)
    {
        try
        {
            var teamMembers = await _projectService.GetProjectTeamMembersAsync(id);
            return Ok(ApiResponse<List<TeamMemberDto>>.Success(teamMembers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project team for {ProjectId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Add team to project
    /// </summary>
    [HttpPost("{id:guid}/teams")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> AddTeamToProject(Guid id, [FromBody] AddTeamToProjectDto dto)
    {
        try
        {
            await _projectService.AddTeamToProjectAsync(id, dto.TeamId);
            return Ok(ApiResponse<string>.Success("Team added to project successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team {TeamId} to project {ProjectId}", dto.TeamId, id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Remove team from project
    /// </summary>
    [HttpDelete("{id:guid}/teams/{teamId:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> RemoveTeamFromProject(Guid id, Guid teamId)
    {
        try
        {
            await _projectService.RemoveTeamFromProjectAsync(id, teamId);
            return Ok(ApiResponse<string>.Success("Team removed from project successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team {TeamId} from project {ProjectId}", teamId, id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
    
    private bool IsOwnerOrManager()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        return role == "Owner" || role == "Manager";
    }
}
