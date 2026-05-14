using Events_GSS.Data.Models;

namespace ChatAndEvents.Web.Models;

public class NotificationsViewModel
{
    public List<Notification> Notifications { get; set; } = new();
    public bool IsEmpty => Notifications.Count == 0;
    public string? ErrorMessage { get; set; }
}