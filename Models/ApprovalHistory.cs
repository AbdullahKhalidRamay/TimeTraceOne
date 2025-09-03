using System.ComponentModel.DataAnnotations;

namespace TimeTraceOne.Models;

public class ApprovalHistory : BaseEntity
{
    [Required]
    public Guid EntryId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string PreviousStatus { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string NewStatus { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Message { get; set; }
    
    [Required]
    public Guid ApprovedBy { get; set; }
    
    // Navigation properties
    public virtual TimeEntry Entry { get; set; } = null!;
    public virtual User Approver { get; set; } = null!;
}
