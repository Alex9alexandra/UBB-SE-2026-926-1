using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.Web.Controllers;

public class MainWindowController : Controller
{
    private readonly IUserService _userService;

    public MainWindowController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string activeSection = "Conversations")
    {
        var currentUser = await _userService.GetCurrentUser();

        return View(new MainWindowViewModel
        {
            CurrentUserId = currentUser.UserId,
            CurrentUsername = currentUser.Name,
            ActiveSection = activeSection
        });
    }
}
