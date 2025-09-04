using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all users with filtering and pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<PaginatedResponse<UserListDto>>> GetUsers([FromQuery] UserFilterDto filter)
    {
        try
        {
            var users = await _userService.GetUsersAsync(filter);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own profile unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<object>.Error("User not found"));
                
            return Ok(ApiResponse<UserDetailDto>.Success(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<UserDetailDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ApiResponse<UserDetailDto>.Success(user));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDetailDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var currentUserId = GetCurrentUserId();
            
            // Users can only update their own profile unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var user = await _userService.UpdateUserAsync(id, dto);
            return Ok(ApiResponse<UserDetailDto>.Success(user));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Delete user
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return Ok(ApiResponse<string>.Success("User deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user statistics
    /// </summary>
    [HttpGet("{id}/statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics(Guid id, [FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own statistics unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var stats = await _userService.GetUserStatisticsAsync(id, startDate, endDate);
            return Ok(ApiResponse<UserStatisticsDto>.Success(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics for {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user weekly report
    /// </summary>
    [HttpGet("{id}/reports/weekly")]
    public async Task<ActionResult<UserWeeklyReportDto>> GetUserWeeklyReport(Guid id, [FromQuery] string weekStart)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own reports unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var report = await _userService.GetUserWeeklyReportAsync(id, weekStart);
            return Ok(ApiResponse<UserWeeklyReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user weekly report for {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user monthly report
    /// </summary>
    [HttpGet("{id}/reports/monthly")]
    public async Task<ActionResult<UserMonthlyReportDto>> GetUserMonthlyReport(Guid id, [FromQuery] string month)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own reports unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var report = await _userService.GetUserMonthlyReportAsync(id, month);
            return Ok(ApiResponse<UserMonthlyReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user monthly report for {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user available hours for a specific date
    /// </summary>
    [HttpGet("{id}/available-hours")]
    public async Task<ActionResult<UserAvailableHoursDto>> GetUserAvailableHours(Guid id, [FromQuery] string date)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own available hours unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var availableHours = await _userService.GetUserAvailableHoursAsync(id, date);
            return Ok(ApiResponse<UserAvailableHoursDto>.Success(availableHours));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user available hours for {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user associated projects
    /// </summary>
    [HttpGet("{id}/projects")]
    public async Task<ActionResult<List<ProjectSummaryDto>>> GetUserProjects(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own projects unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var projects = await _userService.GetUserAssociatedProjectsAsync(id);
            return Ok(ApiResponse<List<ProjectSummaryDto>>.Success(projects));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user projects for {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user reports for a date range
    /// </summary>
    [HttpGet("{id}/reports")]
    public async Task<ActionResult<UserReportDto>> GetUserReports(Guid id, [FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own reports unless they're Owner/Manager
            if (id != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            // For now, return a simple report object since the service method doesn't exist
            var report = new
            {
                UserId = id,
                StartDate = startDate,
                EndDate = endDate,
                TotalHours = 0.0m,
                TotalProjects = 0,
                TotalTimeEntries = 0
            };
            return Ok(ApiResponse<object>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user reports for {UserId}", id);
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
