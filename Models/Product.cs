using System.ComponentModel.DataAnnotations;

namespace TimeTraceOne.Models;

public class Product : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? ProductDescription { get; set; }
    
    public bool IsBillable { get; set; }
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    
    [Required]
    public Guid CreatedBy { get; set; }
    
    // Navigation properties
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<TeamProduct> TeamProducts { get; set; } = new List<TeamProduct>();
}
