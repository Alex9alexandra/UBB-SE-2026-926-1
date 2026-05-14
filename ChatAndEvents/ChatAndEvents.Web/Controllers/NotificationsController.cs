using Events_GSS.Data.Services.notificationServices;
using Events_GSS.Data.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.Web.Controllers;

public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly CurrentUserContext _currentUserContext;

    public NotificationsController(INotificationService notificationService, CurrentUserContext currentUserContext)
    {
        _notificationService = notificationService;
        _currentUserContext = currentUserContext;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new NotificationsViewModel();
        try
        {
            viewModel.Notifications = await _notificationService.GetNotificationsAsync(_currentUserContext.UserId);
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Could not load notifications: {ex.Message}";
        }
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int notificationId)
    {
        await _notificationService.DeleteAsync(notificationId);
        return RedirectToAction(nameof(Index));
    }
}