using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all products with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProductDto>>> GetProducts([FromQuery] ProductFilterDto filter)
    {
        try
        {
            var products = await _productService.GetProductsAsync(filter);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(ApiResponse<object>.Error("Product not found"));
                
            return Ok(ApiResponse<ProductDto>.Success(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var currentUserId = GetCurrentUserId();
            var product = await _productService.CreateProductAsync(dto, currentUserId);
            
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ApiResponse<ProductDto>.Success(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Error("Invalid input data"));
                
            var product = await _productService.UpdateProductAsync(id, dto);
            return Ok(ApiResponse<ProductDto>.Success(product));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return Ok(ApiResponse<string>.Success("Product deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
