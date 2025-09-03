using System.ComponentModel.DataAnnotations;

namespace TimeTraceOne.Models;

public class Notification : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    public NotificationType Type { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public Guid? RelatedEntryId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual TimeEntry? RelatedEntry { get; set; }
}
