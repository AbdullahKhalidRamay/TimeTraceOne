using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTraceOne.Models;

public class TimeEntry : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [Range(0, 24)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal ActualHours { get; set; }
    
    [Required]
    [Range(0, 24)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal BillableHours { get; set; }
    
    [Required]
    [Range(0, 24)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal TotalHours { get; set; }
    
    [Required]
    [Range(0, 24)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal AvailableHours { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Task { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(4000)]
    public string ProjectDetails { get; set; } = string.Empty; // JSON string
    
    public bool IsBillable { get; set; }
    
    public EntryStatus Status { get; set; } = EntryStatus.Pending;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<ApprovalHistory> ApprovalHistory { get; set; } = new List<ApprovalHistory>();
}
