using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class DepartmentService : IDepartmentService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<DepartmentService> _logger;
    
    public DepartmentService(TimeFlowDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<DepartmentDto>> GetDepartmentsAsync(DepartmentFilterDto filter)
    {
        var query = _context.Departments
            .Include(d => d.Creator)
            .Include(d => d.Teams)
            .AsQueryable();
            
        // Apply filters
        if (filter.IsBillable.HasValue)
            query = query.Where(d => d.IsBillable == filter.IsBillable.Value);
            
        if (filter.Status.HasValue)
            query = query.Where(d => d.Status == filter.Status.Value);
            
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(d => d.Name.Contains(filter.Search) || 
                                   (d.DepartmentDescription != null && d.DepartmentDescription.Contains(filter.Search)));
            
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / filter.Limit);
        
        var departments = await query
            .OrderBy(d => d.Name)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        var departmentDtos = departments.Select(MapToDepartmentDto).ToList();
        
        return PaginatedResponse<DepartmentDto>.CreateSuccess(departmentDtos, filter.Page, filter.Limit, total, totalPages);
    }
    
    public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id)
    {
        var department = await _context.Departments
            .Include(d => d.Creator)
            .Include(d => d.Teams)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        return department != null ? MapToDepartmentDto(department) : null;
    }
    
    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto, Guid createdBy)
    {
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            DepartmentDescription = dto.DepartmentDescription,
            IsBillable = dto.IsBillable,
            Status = ProjectStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created department {DepartmentId} with name {DepartmentName}", department.Id, department.Name);
        
        return await GetDepartmentByIdAsync(department.Id) ?? throw new InvalidOperationException("Failed to retrieve created department");
    }
    
    public async Task<DepartmentDto> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto dto)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            throw new InvalidOperationException("Department not found");
            
        if (dto.Name != null)
            department.Name = dto.Name;
            
        if (dto.DepartmentDescription != null)
            department.DepartmentDescription = dto.DepartmentDescription;
            
        if (dto.IsBillable.HasValue)
            department.IsBillable = dto.IsBillable.Value;
            
        if (dto.Status.HasValue)
            department.Status = dto.Status.Value;
            
        department.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated department {DepartmentId}", id);
        
        return await GetDepartmentByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated department");
    }
    
    public async Task DeleteDepartmentAsync(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            throw new InvalidOperationException("Department not found");
            
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted department {DepartmentId}", id);
    }
    
    private static DepartmentDto MapToDepartmentDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            DepartmentDescription = department.DepartmentDescription,
            IsBillable = department.IsBillable,
            Status = department.Status,
            CreatedBy = department.CreatedBy,
            CreatorName = department.Creator?.Name ?? string.Empty,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt,
            TeamCount = department.Teams.Count,
            MemberCount = department.Teams.Sum(t => t.Members.Count)
        };
    }
}
