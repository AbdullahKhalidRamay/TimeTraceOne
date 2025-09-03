using System.ComponentModel.DataAnnotations;
using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class CreateProductDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? ProductDescription { get; set; }
    
    public bool IsBillable { get; set; } = true;
    
    public List<Guid>? TeamIds { get; set; } = new List<Guid>();
}

public class UpdateProductDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(2000)]
    public string? ProductDescription { get; set; }
    
    public bool? IsBillable { get; set; }
    
    public ProjectStatus? Status { get; set; }
    
    public List<Guid>? TeamIds { get; set; }
}

public class ProductDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public bool IsBillable { get; set; }
    public ProjectStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public List<Guid> TeamIds { get; set; } = new List<Guid>();
    public List<string> TeamNames { get; set; } = new List<string>();
}

public class ProductFilterDto
{
    public bool? IsBillable { get; set; }
    public ProjectStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class ProductSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBillable { get; set; }
    public ProjectStatus Status { get; set; }
}
