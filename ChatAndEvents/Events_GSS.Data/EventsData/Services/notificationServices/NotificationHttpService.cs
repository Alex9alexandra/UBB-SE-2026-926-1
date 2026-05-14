using System.Net.Http.Json;
using Events_GSS.Data.Models; 
using Events_GSS.Data.Services.notificationServices;

namespace Events_GSS.Data.Services.notificationServices;

public class NotificationHttpService : INotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Notification>> GetNotificationsAsync(Guid userId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<Notification>>($"api/Notifications/{userId}");
        return result ?? new List<Notification>();
    }

    public async Task DeleteAsync(int notificationId)
    {
        await _httpClient.DeleteAsync($"api/Notifications/{notificationId}");
    }

    public async Task NotifyAsync(Guid userId, string title, string description)
    {
        await _httpClient.PostAsync($"api/Notifications?userId={userId}&title={Uri.EscapeDataString(title)}&description={Uri.EscapeDataString(description)}", null);
    }
}