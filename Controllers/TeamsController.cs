using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamsController> _logger;
    
    public TeamsController(ITeamService teamService, ILogger<TeamsController> logger)
    {
        _teamService = teamService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all teams with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<TeamDto>>> GetTeams([FromQuery] TeamFilterDto filter)
    {
        try
        {
            var teams = await _teamService.GetTeamsAsync(filter);
            return Ok(teams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get team by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid id)
    {
        try
        {
            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null)
                return NotFound(ApiResponse<object>.Error("Team not found"));
                
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Create a new team
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var currentUserId = GetCurrentUserId();
            var team = await _teamService.CreateTeamAsync(dto, currentUserId);
            
            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, ApiResponse<TeamDto>.Success(team));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Update team
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> UpdateTeam(Guid id, [FromBody] UpdateTeamDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var team = await _teamService.UpdateTeamAsync(id, dto);
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Delete team
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> DeleteTeam(Guid id)
    {
        try
        {
            await _teamService.DeleteTeamAsync(id);
            return Ok(ApiResponse<string>.Success("Team deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Add member to team
    /// </summary>
    [HttpPost("{id}/members")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> AddMember(Guid id, [FromBody] AddTeamMemberDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var team = await _teamService.AddMemberToTeamAsync(id, dto);
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Remove member from team
    /// </summary>
    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> RemoveMember(Guid id, Guid userId)
    {
        try
        {
            var team = await _teamService.RemoveMemberFromTeamAsync(id, userId);
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Update team leader
    /// </summary>
    [HttpPut("{id}/leader")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> UpdateLeader(Guid id, [FromBody] UpdateTeamLeaderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var team = await _teamService.UpdateTeamLeaderAsync(id, dto);
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team leader for {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Associate team with project
    /// </summary>
    [HttpPost("{id}/projects")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> AssociateProject(Guid id, [FromBody] AssociateTeamProjectDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var team = await _teamService.AssociateTeamWithProjectAsync(id, dto);
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating project with team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Associate team with product
    /// </summary>
    [HttpPost("{id}/products")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> AssociateProduct(Guid id, [FromBody] AssociateTeamProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var team = await _teamService.AssociateTeamWithProductAsync(id, dto);
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating product with team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Associate team with department
    /// </summary>
    [HttpPost("{id}/departments")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamDto>> AssociateDepartment(Guid id, [FromBody] AssociateTeamDepartmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var team = await _teamService.AssociateTeamWithDepartmentAsync(id, dto);
            return Ok(ApiResponse<TeamDto>.Success(team));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating department with team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user teams
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<TeamSummaryDto>>> GetUserTeams(Guid userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own teams unless they're Owner/Manager
            if (userId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var teams = await _teamService.GetUserTeamsAsync(userId);
            return Ok(ApiResponse<List<TeamSummaryDto>>.Success(teams));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user teams for {UserId}", userId);
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
