using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTraceOne.DTOs;
using TimeTraceOne.Services;

namespace TimeTraceOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;
    
    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get user notifications with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NotificationListResponseDto>> GetNotifications([FromQuery] NotificationFilterDto filter)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(currentUserId, filter);
            return Ok(ApiResponse<NotificationListResponseDto>.Success(notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Get notification by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationDto>> GetNotification(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound(ApiResponse<object>.Error("Notification not found"));
                
            // Check if user owns this notification
            var currentUserId = GetCurrentUserId();
            if (notification.UserId != currentUserId)
                return Forbid();
                
            return Ok(ApiResponse<NotificationDto>.Success(notification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification {NotificationId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<ActionResult<NotificationDto>> MarkAsRead(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound(ApiResponse<object>.Error("Notification not found"));
                
            // Check if user owns this notification
            var currentUserId = GetCurrentUserId();
            if (notification.UserId != currentUserId)
                return Forbid();
                
            var updatedNotification = await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok(ApiResponse<NotificationDto>.Success(updatedNotification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Create notification
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var notification = await _notificationService.CreateNotificationAsync(dto, currentUserId);
            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, 
                ApiResponse<NotificationDto>.Success(notification, "Notification created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    public async Task<ActionResult<MarkAllNotificationsReadDto>> MarkAllAsRead()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _notificationService.MarkAllNotificationsAsReadAsync(currentUserId);
            return Ok(ApiResponse<MarkAllNotificationsReadDto>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    /// <summary>
    /// Delete notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
                return NotFound(ApiResponse<object>.Error("Notification not found"));
                
            // Check if user owns this notification
            var currentUserId = GetCurrentUserId();
            if (notification.UserId != currentUserId)
                return Forbid();
                
            await _notificationService.DeleteNotificationAsync(id);
            return Ok(ApiResponse<string>.Success("Notification deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return StatusCode(500, ApiResponse<object>.Error("Internal server error"));
        }
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
