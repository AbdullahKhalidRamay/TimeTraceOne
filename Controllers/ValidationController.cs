using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ValidationController : ControllerBase
{
    private readonly IValidationService _validationService;
    private readonly ILogger<ValidationController> _logger;
    
    public ValidationController(IValidationService validationService, ILogger<ValidationController> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }
    
    /// <summary>
    /// Validate time entry
    /// </summary>
    [HttpPost("time-entry")]
    public async Task<ActionResult<TimeEntryValidationResultDto>> ValidateTimeEntry([FromBody] TimeEntryValidationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var currentUserId = GetCurrentUserId();
            
            // Users can only validate their own time entries unless they're Owner/Manager
            if (dto.UserId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var result = await _validationService.ValidateTimeEntryAsync(dto);
            return Ok(ApiResponse<TimeEntryValidationResultDto>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating time entry");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user available hours for a specific date
    /// </summary>
    [HttpGet("available-hours/{userId:guid}")]
    public async Task<ActionResult<UserAvailableHoursDto>> GetUserAvailableHours(Guid userId, [FromQuery] string date)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own available hours unless they're Owner/Manager
            if (userId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var availableHours = await _validationService.GetUserAvailableHoursAsync(userId, date);
            return Ok(ApiResponse<UserAvailableHoursDto>.Success(availableHours));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user available hours for {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Validate user access to a resource
    /// </summary>
    [HttpGet("user-access/{userId:guid}")]
    public async Task<ActionResult<bool>> ValidateUserAccess(Guid userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only validate their own access unless they're Owner/Manager
            if (userId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var hasAccess = await _validationService.ValidateUserAccessAsync(currentUserId, userId);
            return Ok(ApiResponse<bool>.Success(hasAccess));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user access for {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Validate team access
    /// </summary>
    [HttpGet("team-access/{teamId:guid}")]
    public async Task<ActionResult<bool>> ValidateTeamAccess(Guid teamId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var hasAccess = await _validationService.ValidateTeamAccessAsync(currentUserId, teamId);
            
            return Ok(ApiResponse<bool>.Success(hasAccess));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating team access for {TeamId}", teamId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Validate project access
    /// </summary>
    [HttpGet("project-access/{projectId:guid}")]
    public async Task<ActionResult<bool>> ValidateProjectAccess(Guid projectId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var hasAccess = await _validationService.ValidateProjectAccessAsync(currentUserId, projectId);
            
            return Ok(ApiResponse<bool>.Success(hasAccess));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating project access for {ProjectId}", projectId);
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
