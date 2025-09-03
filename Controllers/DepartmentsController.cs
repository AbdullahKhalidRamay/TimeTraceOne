using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;
    
    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all departments with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<DepartmentDto>>> GetDepartments([FromQuery] DepartmentFilterDto filter)
    {
        try
        {
            var departments = await _departmentService.GetDepartmentsAsync(filter);
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(Guid id)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
                return NotFound(ApiResponse<object>.Error("Department not found"));
                
            return Ok(ApiResponse<DepartmentDto>.Success(department));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department {DepartmentId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var currentUserId = GetCurrentUserId();
            var department = await _departmentService.CreateDepartmentAsync(dto, currentUserId);
            
            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, ApiResponse<DepartmentDto>.Success(department));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Update department
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var department = await _departmentService.UpdateDepartmentAsync(id, dto);
            return Ok(ApiResponse<DepartmentDto>.Success(department));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {DepartmentId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Delete department
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> DeleteDepartment(Guid id)
    {
        try
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return Ok(ApiResponse<string>.Success("Department deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {DepartmentId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
