using ChatAndEvents.Data.EventsData.Services.notificationServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> Notify(
        [FromQuery] Guid userId,
        [FromQuery] string title,
        [FromQuery] string description)
    {
        await _notificationService.NotifyAsync(userId, title, description);
        return NoContent();
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetNotifications(Guid userId)
    {
        var notifications = await _notificationService.GetNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        await _notificationService.DeleteAsync(notificationId);
        return NoContent();
    }
}