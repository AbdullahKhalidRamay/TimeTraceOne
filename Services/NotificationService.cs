using Microsoft.EntityFrameworkCore;
using TimeTraceOne.Data;
using TimeTraceOne.DTOs;
using TimeTraceOne.Models;

namespace TimeTraceOne.Services;

public class NotificationService : INotificationService
{
    private readonly TimeFlowDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    
    public NotificationService(TimeFlowDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<NotificationListResponseDto> GetUserNotificationsAsync(Guid userId, NotificationFilterDto filter)
    {
        var query = _context.Notifications
            .Include(n => n.User)
            .Include(n => n.RelatedEntry)
            .Where(n => n.UserId == userId);
            
        // Apply filters
        if (filter.Type?.Any() == true)
            query = query.Where(n => filter.Type.Contains(n.Type));
            
        if (filter.IsRead.HasValue)
            query = query.Where(n => n.IsRead == filter.IsRead.Value);
            
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)total / filter.Limit);
        var unreadCount = await query.CountAsync(n => !n.IsRead);
        
        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .ToListAsync();
            
        var notificationDtos = notifications.Select(MapToNotificationDto).ToList();
        
        var pagination = new PaginationDto
        {
            Page = filter.Page,
            Limit = filter.Limit,
            Total = total,
            TotalPages = totalPages
        };
        
        return new NotificationListResponseDto
        {
            Notifications = notificationDtos,
            UnreadCount = unreadCount,
            Pagination = pagination
        };
    }
    
    public async Task<NotificationDto?> GetNotificationByIdAsync(Guid id)
    {
        var notification = await _context.Notifications
            .Include(n => n.User)
            .Include(n => n.RelatedEntry)
            .FirstOrDefaultAsync(n => n.Id == id);
            
        return notification != null ? MapToNotificationDto(notification) : null;
    }
    
    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, Guid userId)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            RelatedEntryId = dto.RelatedEntryId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created notification {NotificationId} for user {UserId}", notification.Id, userId);
        
        return await GetNotificationByIdAsync(notification.Id) ?? throw new InvalidOperationException("Failed to retrieve created notification");
    }
    
    public async Task<NotificationDto> MarkNotificationAsReadAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            throw new InvalidOperationException("Notification not found");
            
        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Marked notification {NotificationId} as read", id);
        
        return await GetNotificationByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated notification");
    }
    
    public async Task<MarkAllNotificationsReadDto> MarkAllNotificationsAsReadAsync(Guid userId)
    {
        var updatedCount = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.UpdatedAt, DateTime.UtcNow));
        
        _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", updatedCount, userId);
        
        return new MarkAllNotificationsReadDto { UpdatedCount = updatedCount };
    }
    
    public async Task DeleteNotificationAsync(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            throw new InvalidOperationException("Notification not found");
            
        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted notification {NotificationId}", id);
    }
    
    public async Task<NotificationDto> CreateApprovalNotificationAsync(Guid userId, Guid entryId, string message)
    {
        return await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Timesheet Approved",
            Message = message,
            Type = NotificationType.Approval,
            RelatedEntryId = entryId
        }, userId);
    }
    
    public async Task<NotificationDto> CreateRejectionNotificationAsync(Guid userId, Guid entryId, string message)
    {
        return await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = "Timesheet Rejected",
            Message = message,
            Type = NotificationType.Rejection,
            RelatedEntryId = entryId
        }, userId);
    }
    
    public async Task<NotificationDto> CreateReminderNotificationAsync(Guid userId, string title, string message)
    {
        return await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = NotificationType.Reminder
        }, userId);
    }
    
    public async Task<NotificationDto> CreateSystemNotificationAsync(Guid userId, string title, string message)
    {
        return await CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = NotificationType.System
        }, userId);
    }
    
    private static NotificationDto MapToNotificationDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            UserName = notification.User?.Name ?? string.Empty,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            RelatedEntryId = notification.RelatedEntryId,
            RelatedEntryTask = notification.RelatedEntry?.Task,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt
        };
    }
}
