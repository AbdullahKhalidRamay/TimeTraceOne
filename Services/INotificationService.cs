using TimeTraceOne.DTOs;

namespace TimeTraceOne.Services;

public interface INotificationService
{
    Task<NotificationListResponseDto> GetUserNotificationsAsync(Guid userId, NotificationFilterDto filter);
    Task<NotificationDto?> GetNotificationByIdAsync(Guid id);
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, Guid userId);
    Task<NotificationDto> MarkNotificationAsReadAsync(Guid id);
    Task<MarkAllNotificationsReadDto> MarkAllNotificationsAsReadAsync(Guid userId);
    Task DeleteNotificationAsync(Guid id);
    
    // System notifications
    Task<NotificationDto> CreateApprovalNotificationAsync(Guid userId, Guid entryId, string message);
    Task<NotificationDto> CreateRejectionNotificationAsync(Guid userId, Guid entryId, string message);
    Task<NotificationDto> CreateReminderNotificationAsync(Guid userId, string title, string message);
    Task<NotificationDto> CreateSystemNotificationAsync(Guid userId, string title, string message);
}
