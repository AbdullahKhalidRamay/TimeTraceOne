using System.ComponentModel.DataAnnotations;
using TimeTraceOne.Models;

namespace TimeTraceOne.DTOs;

public class CreateNotificationDto
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
    
    public Guid? RelatedEntryId { get; set; }
}

public class NotificationDto : BaseDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public Guid? RelatedEntryId { get; set; }
    public string? RelatedEntryTask { get; set; }
}

public class NotificationFilterDto
{
    public List<NotificationType>? Type { get; set; }
    public bool? IsRead { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}

public class NotificationListResponseDto
{
    public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
    public int UnreadCount { get; set; }
    public PaginationDto Pagination { get; set; } = new PaginationDto();
}

public class PaginationDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}

public class MarkNotificationReadDto
{
    public Guid NotificationId { get; set; }
}

public class MarkAllNotificationsReadDto
{
    public int UpdatedCount { get; set; }
}
