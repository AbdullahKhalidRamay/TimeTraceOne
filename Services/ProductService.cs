using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class ProductService : IProductService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(TimeFlowDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<PaginatedResponse<ProductDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Creator)
            .Include(p => p.TeamProducts)
                .ThenInclude(tp => tp.Team)
            .AsQueryable();
            
        // Apply filters
        if (filter.IsBillable.HasValue)
            query = query.Where(p => p.IsBillable == filter.IsBillable.Value);
            
        if (filter.Status.HasValue)
            query = query.Where(p => p.Status == filter.Status.Value);
            
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search) || 
                                   (p.ProductDescription != null && p.ProductDescription.Contains(filter.Search)));
            
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / filter.Limit);
        
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        var productDtos = products.Select(MapToProductDto).ToList();
        
        return PaginatedResponse<ProductDto>.CreateSuccess(productDtos, filter.Page, filter.Limit, total, totalPages);
    }
    
    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Creator)
            .Include(p => p.TeamProducts)
                .ThenInclude(tp => tp.Team)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        return product != null ? MapToProductDto(product) : null;
    }
    
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, Guid createdBy)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            ProductDescription = dto.ProductDescription,
            IsBillable = dto.IsBillable,
            Status = ProjectStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Products.Add(product);
        
        // Add team associations if specified
        if (dto.TeamIds?.Any() == true)
        {
            foreach (var teamId in dto.TeamIds)
            {
                var teamProduct = new TeamProduct
                {
                    TeamId = teamId,
                    ProductId = product.Id
                };
                _context.TeamProducts.Add(teamProduct);
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created product {ProductId} with name {ProductName}", product.Id, product.Name);
        
        return await GetProductByIdAsync(product.Id) ?? throw new InvalidOperationException("Failed to retrieve created product");
    }
    
    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.TeamProducts)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (product == null)
            throw new InvalidOperationException("Product not found");
            
        if (dto.Name != null)
            product.Name = dto.Name;
            
        if (dto.ProductDescription != null)
            product.ProductDescription = dto.ProductDescription;
            
        if (dto.IsBillable.HasValue)
            product.IsBillable = dto.IsBillable.Value;
            
        if (dto.Status.HasValue)
            product.Status = dto.Status.Value;
            
        product.UpdatedAt = DateTime.UtcNow;
        
        // Update team associations if specified
        if (dto.TeamIds != null)
        {
            // Remove existing associations
            var existingAssociations = product.TeamProducts.ToList();
            foreach (var association in existingAssociations)
            {
                _context.TeamProducts.Remove(association);
            }
            
            // Add new associations
            foreach (var teamId in dto.TeamIds)
            {
                var teamProduct = new TeamProduct
                {
                    TeamId = teamId,
                    ProductId = product.Id
                };
                _context.TeamProducts.Add(teamProduct);
            }
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated product {ProductId}", id);
        
        return await GetProductByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated product");
    }
    
    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            throw new InvalidOperationException("Product not found");
            
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted product {ProductId}", id);
    }
    
    private static ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            ProductDescription = product.ProductDescription,
            IsBillable = product.IsBillable,
            Status = product.Status,
            CreatedBy = product.CreatedBy,
            CreatorName = product.Creator?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            TeamIds = product.TeamProducts.Select(tp => tp.TeamId).ToList(),
            TeamNames = product.TeamProducts.Select(tp => tp.Team.Name).ToList()
        };
    }
}
