using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;
    
    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all users report for a date range
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<List<UserReportDto>>> GetUsersReport([FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var reports = await _reportService.GetUsersReportAsync(startDate, endDate);
            return Ok(ApiResponse<List<UserReportDto>>.Success(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users report");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user report for a date range
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserReportDto>> GetUserReport(Guid userId, [FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own reports unless they're Owner/Manager
            if (userId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var report = await _reportService.GetUserReportAsync(userId, startDate, endDate);
            return Ok(ApiResponse<UserReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user report for {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user weekly report
    /// </summary>
    [HttpGet("users/{userId}/weekly")]
    public async Task<ActionResult<UserWeeklyReportDto>> GetUserWeeklyReport(Guid userId, [FromQuery] string weekStart)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own reports unless they're Owner/Manager
            if (userId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var report = await _reportService.GetUserWeeklyReportAsync(userId, weekStart);
            return Ok(ApiResponse<UserWeeklyReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user weekly report for {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get user monthly report
    /// </summary>
    [HttpGet("users/{userId}/monthly")]
    public async Task<ActionResult<UserMonthlyReportDto>> GetUserMonthlyReport(Guid userId, [FromQuery] string month)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only view their own reports unless they're Owner/Manager
            if (userId != currentUserId && !IsOwnerOrManager())
            {
                return Forbid();
            }
            
            var report = await _reportService.GetUserMonthlyReportAsync(userId, month);
            return Ok(ApiResponse<UserMonthlyReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user monthly report for {UserId}", userId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get all teams report for a date range
    /// </summary>
    [HttpGet("teams")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<List<TeamReportDto>>> GetTeamsReport([FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var reports = await _reportService.GetTeamsReportAsync(startDate, endDate);
            return Ok(ApiResponse<List<TeamReportDto>>.Success(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teams report");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get team report for a date range
    /// </summary>
    [HttpGet("teams/{teamId}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamReportDto>> GetTeamReport(Guid teamId, [FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var report = await _reportService.GetTeamReportAsync(teamId, startDate, endDate);
            return Ok(ApiResponse<TeamReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team report for {TeamId}", teamId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get team weekly report
    /// </summary>
    [HttpGet("teams/{teamId}/weekly")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TeamReportDto>> GetTeamWeeklyReport(Guid teamId, [FromQuery] string weekStart)
    {
        try
        {
            var report = await _reportService.GetTeamWeeklyReportAsync(teamId, weekStart);
            return Ok(ApiResponse<TeamReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team weekly report for {TeamId}", teamId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get system overview report
    /// </summary>
    [HttpGet("system")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<SystemOverviewDto>> GetSystemOverview([FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var report = await _reportService.GetSystemOverviewAsync(startDate, endDate);
            return Ok(ApiResponse<SystemOverviewDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system overview report");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get all departments performance report
    /// </summary>
    [HttpGet("departments")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<List<DepartmentPerformanceDto>>> GetDepartmentsPerformance([FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var report = await _reportService.GetDepartmentPerformanceAsync(startDate, endDate);
            return Ok(ApiResponse<List<DepartmentPerformanceDto>>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments performance report");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get specific department performance report
    /// </summary>
    [HttpGet("departments/{departmentId:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<DepartmentPerformanceDto>> GetDepartmentPerformance(Guid departmentId, [FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var report = await _reportService.GetDepartmentPerformanceByIdAsync(departmentId, startDate, endDate);
            return Ok(ApiResponse<DepartmentPerformanceDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department performance report for {DepartmentId}", departmentId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get all projects performance report
    /// </summary>
    [HttpGet("projects")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<List<ProjectPerformanceReportDto>>> GetProjectsPerformance([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string? status)
    {
        try
        {
            var report = await _reportService.GetProjectPerformanceAsync(startDate, endDate, status);
            return Ok(ApiResponse<List<ProjectPerformanceReportDto>>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects performance report");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get specific project performance report
    /// </summary>
    [HttpGet("projects/{projectId:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<ProjectPerformanceReportDto>> GetProjectPerformance(Guid projectId, [FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var report = await _reportService.GetProjectPerformanceByIdAsync(projectId, startDate, endDate);
            return Ok(ApiResponse<ProjectPerformanceReportDto>.Success(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project performance report for {ProjectId}", projectId);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Export report to CSV
    /// </summary>
    [HttpGet("export/csv")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> ExportCsv([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string type)
    {
        try
        {
            var request = new ExportRequestDto { StartDate = startDate, EndDate = endDate, ReportType = type };
            var csvData = await _reportService.ExportCsvReportAsync(request);
            var fileName = $"report_{type}_{DateTime.UtcNow:yyyyMMdd}.csv";
            
            return File(csvData, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting CSV report");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Export report to PDF
    /// </summary>
    [HttpGet("export/pdf")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> ExportPdf([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] string type)
    {
        try
        {
            var request = new ExportRequestDto { StartDate = startDate, EndDate = endDate, ReportType = type };
            var pdfData = await _reportService.ExportPdfReportAsync(request);
            var fileName = $"report_{type}_{DateTime.UtcNow:yyyyMMdd}.pdf";
            
            return File(pdfData, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting PDF report");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Search time entries
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<TimeEntrySearchResultDto>> SearchTimeEntries([FromQuery] string query, [FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var results = await _reportService.SearchTimeEntriesAsync(query, startDate, endDate);
            return Ok(ApiResponse<TimeEntrySearchResultDto>.Success(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching time entries with query {Query}", query);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get top projects for a department
    /// </summary>
    [HttpGet("departments/{departmentId:guid}/top-projects")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<List<ProjectPerformanceReportDto>>> GetTopProjectsForDepartment(Guid departmentId, [FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            var projects = await _reportService.GetTopProjectsForDepartmentAsync(departmentId, startDate, endDate);
            return Ok(ApiResponse<List<ProjectPerformanceReportDto>>.Success(projects));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top projects for department {DepartmentId}", departmentId);
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
