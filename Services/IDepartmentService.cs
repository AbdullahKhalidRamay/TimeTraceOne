using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface IDepartmentService
{
    Task<PaginatedResponse<DepartmentDto>> GetDepartmentsAsync(DepartmentFilterDto filter);
    Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id);
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto, Guid createdBy);
    Task<DepartmentDto> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto dto);
    Task DeleteDepartmentAsync(Guid id);
}
