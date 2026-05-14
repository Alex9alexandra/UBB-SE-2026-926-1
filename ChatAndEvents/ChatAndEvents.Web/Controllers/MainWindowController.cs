using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.ChatData.serviceInterfaces.Services; // For IFriendRequestService
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MainWindowController : Controller
{
    private readonly IUserService _userService;
    private readonly IFriendRequestService _friendRequestService;
    
    // TEMPORARILY REMOVED
    // private readonly INotificationService _notificationService; 

    public MainWindowController(
        IUserService userService, 
        IFriendRequestService friendRequestService)
    {
        _userService = userService;
        _friendRequestService = friendRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string activeSection = "Conversations")
    {
        var currentUser = await _userService.GetCurrentUser();
        var friendRequests = await _friendRequestService.GetIncomingRequestsAsync(currentUser.UserId);

        return View(new MainWindowViewModel
        {
            CurrentUserId = currentUser.UserId,
            CurrentUsername = currentUser.Name,
            ActiveSection = activeSection,
            
            UnreadNotificationsCount = 0, // Hardcoded to 0 to bypass the error
            PendingFriendRequestsCount = friendRequests?.Count ?? 0
        });
    }
}