using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface IProductService
{
    Task<PaginatedResponse<ProductDto>> GetProductsAsync(ProductFilterDto filter);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid createdBy);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto);
    Task DeleteProductAsync(Guid id);
}
