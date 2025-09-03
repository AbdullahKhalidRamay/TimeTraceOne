using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;
    private readonly ILogger<TimeEntriesController> _logger;
    
    public TimeEntriesController(ITimeEntryService timeEntryService, ILogger<TimeEntriesController> logger)
    {
        _timeEntryService = timeEntryService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TimeEntryDto>>>> GetTimeEntries([FromQuery] TimeEntryFilterDto filter)
    {
        try
        {
            var timeEntries = await _timeEntryService.GetTimeEntriesAsync(filter);
            return Ok(ApiResponse<List<TimeEntryDto>>.Success(timeEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entries");
            return StatusCode(500, ApiResponse<List<TimeEntryDto>>.Error("Internal server error"));
        }
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> GetTimeEntry(Guid id)
    {
        try
        {
            var timeEntry = await _timeEntryService.GetTimeEntryByIdAsync(id);
            if (timeEntry == null)
                return NotFound(ApiResponse<TimeEntryDto>.Error("Time entry not found"));
                
            return Ok(ApiResponse<TimeEntryDto>.Success(timeEntry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entry {Id}", id);
            return StatusCode(500, ApiResponse<TimeEntryDto>.Error("Internal server error"));
        }
    }
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> CreateTimeEntry([FromBody] CreateTimeEntryDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var timeEntry = await _timeEntryService.CreateTimeEntryAsync(dto, userId);
            return CreatedAtAction(nameof(GetTimeEntry), new { id = timeEntry.Id }, 
                ApiResponse<TimeEntryDto>.Success(timeEntry, "Time entry created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating time entry");
            return StatusCode(500, ApiResponse<TimeEntryDto>.Error("Internal server error"));
        }
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> UpdateTimeEntry(Guid id, [FromBody] UpdateTimeEntryDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var timeEntry = await _timeEntryService.UpdateTimeEntryAsync(id, dto, userId);
            return Ok(ApiResponse<TimeEntryDto>.Success(timeEntry, "Time entry updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<TimeEntryDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating time entry {Id}", id);
            return StatusCode(500, ApiResponse<TimeEntryDto>.Error("Internal server error"));
        }
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTimeEntry(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _timeEntryService.DeleteTimeEntryAsync(id, userId);
            return Ok(ApiResponse<string>.Success("Time entry deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<string>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting time entry {Id}", id);
            return StatusCode(500, ApiResponse<string>.Error("Internal server error"));
        }
    }
    
    [HttpPost("weekly-bulk")]
    public async Task<ActionResult<ApiResponse<WeeklyBulkResponseDto>>> CreateWeeklyBulk([FromBody] WeeklyBulkRequestDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _timeEntryService.CreateWeeklyBulkAsync(dto, userId);
            return Ok(ApiResponse<WeeklyBulkResponseDto>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weekly bulk time entries");
            return StatusCode(500, ApiResponse<WeeklyBulkResponseDto>.Error("Internal server error"));
        }
    }
    
    [HttpGet("weekly/{weekStart}")]
    public async Task<ActionResult<ApiResponse<WeeklyTimeEntriesDto>>> GetWeeklyTimeEntries(string weekStart, [FromQuery] Guid? userId)
    {
        try
        {
            var weeklyEntries = await _timeEntryService.GetWeeklyTimeEntriesAsync(weekStart, userId);
            return Ok(ApiResponse<WeeklyTimeEntriesDto>.Success(weeklyEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weekly time entries for week {WeekStart}", weekStart);
            return StatusCode(500, ApiResponse<WeeklyTimeEntriesDto>.Error("Internal server error"));
        }
    }
    
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Manager,Owner")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> ApproveTimeEntry(Guid id, [FromBody] ApprovalRequestDto request)
    {
        try
        {
            var approverId = GetCurrentUserId();
            var timeEntry = await _timeEntryService.ApproveTimeEntryAsync(id, request.Message, approverId);
            return Ok(ApiResponse<TimeEntryDto>.Success(timeEntry, "Time entry approved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<TimeEntryDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving time entry {Id}", id);
            return StatusCode(500, ApiResponse<TimeEntryDto>.Error("Internal server error"));
        }
    }
    
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Manager,Owner")]
    public async Task<ActionResult<ApiResponse<TimeEntryDto>>> RejectTimeEntry(Guid id, [FromBody] RejectionRequestDto request)
    {
        try
        {
            var rejectorId = GetCurrentUserId();
            var timeEntry = await _timeEntryService.RejectTimeEntryAsync(id, request.Message, rejectorId);
            return Ok(ApiResponse<TimeEntryDto>.Success(timeEntry, "Time entry rejected successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<TimeEntryDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting time entry {Id}", id);
            return StatusCode(500, ApiResponse<TimeEntryDto>.Error("Internal server error"));
        }
    }
    
    [HttpGet("status/{date}")]
    public async Task<ActionResult<ApiResponse<TimeEntryStatusDto>>> GetTimeEntryStatus(string date, [FromQuery] Guid? userId)
    {
        try
        {
            var status = await _timeEntryService.GetTimeEntryStatusAsync(date, userId);
            return Ok(ApiResponse<TimeEntryStatusDto>.Success(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entry status for date {Date}", date);
            return StatusCode(500, ApiResponse<TimeEntryStatusDto>.Error("Internal server error"));
        }
    }
    
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<TimeEntrySearchResultDto>>> SearchTimeEntries([FromQuery] string q, [FromQuery] Guid? userId, [FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var results = await _timeEntryService.SearchTimeEntriesAsync(q, userId, startDate, endDate);
            return Ok(ApiResponse<TimeEntrySearchResultDto>.Success(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching time entries with query {Query}", q);
            return StatusCode(500, ApiResponse<TimeEntrySearchResultDto>.Error("Internal server error"));
        }
    }
    
    [HttpGet("filter")]
    public async Task<ActionResult<ApiResponse<TimeEntryFilterResultDto>>> FilterTimeEntries([FromQuery] TimeEntryAdvancedFilterDto filter)
    {
        try
        {
            var results = await _timeEntryService.FilterTimeEntriesAsync(filter);
            return Ok(ApiResponse<TimeEntryFilterResultDto>.Success(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering time entries");
            return StatusCode(500, ApiResponse<TimeEntryFilterResultDto>.Error("Internal server error"));
        }
    }
    
    [HttpGet("date/{date}")]
    public async Task<ActionResult<ApiResponse<List<TimeEntryDto>>>> GetTimeEntriesByDate(string date, [FromQuery] Guid? userId)
    {
        try
        {
            var timeEntries = await _timeEntryService.GetTimeEntriesByDateAsync(date, userId);
            return Ok(ApiResponse<List<TimeEntryDto>>.Success(timeEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entries for date {Date}", date);
            return StatusCode(500, ApiResponse<List<TimeEntryDto>>.Error("Internal server error"));
        }
    }
    
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<List<TimeEntryDto>>>> GetTimeEntriesByUser(Guid userId, [FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var timeEntries = await _timeEntryService.GetTimeEntriesByUserAsync(userId, startDate, endDate);
            return Ok(ApiResponse<List<TimeEntryDto>>.Success(timeEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entries for user {UserId}", userId);
            return StatusCode(500, ApiResponse<List<TimeEntryDto>>.Error("Internal server error"));
        }
    }
    
    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<ApiResponse<List<TimeEntryDto>>>> GetTimeEntriesByProject(Guid projectId, [FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        try
        {
            var timeEntries = await _timeEntryService.GetTimeEntriesByProjectAsync(projectId, startDate, endDate);
            return Ok(ApiResponse<List<TimeEntryDto>>.Success(timeEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entries for project {ProjectId}", projectId);
            return StatusCode(500, ApiResponse<List<TimeEntryDto>>.Error("Internal server error"));
        }
    }
    
    [HttpGet("range")]
    public async Task<ActionResult<ApiResponse<List<TimeEntryDto>>>> GetTimeEntriesByDateRange([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] Guid? userId, [FromQuery] Guid? projectId)
    {
        try
        {
            var timeEntries = await _timeEntryService.GetTimeEntriesByDateRangeAsync(startDate, endDate, userId, projectId);
            return Ok(ApiResponse<List<TimeEntryDto>>.Success(timeEntries));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time entries for date range {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, ApiResponse<List<TimeEntryDto>>.Error("Internal server error"));
        }
    }
    
    [HttpPut("weekly/{weekStart}")]
    public async Task<ActionResult<ApiResponse<WeeklyBulkResponseDto>>> UpdateWeeklyTimeEntries(string weekStart, [FromBody] UpdateWeeklyTimeEntriesDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _timeEntryService.UpdateWeeklyTimeEntriesAsync(weekStart, dto, userId);
            return Ok(ApiResponse<WeeklyBulkResponseDto>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weekly time entries for week {WeekStart}", weekStart);
            return StatusCode(500, ApiResponse<WeeklyBulkResponseDto>.Error("Internal server error"));
        }
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }
}

public class ApprovalRequestDto
{
    public string? Message { get; set; }
}

public class RejectionRequestDto
{
    public string Message { get; set; } = string.Empty;
}
