using ChatAndEvents.Data.EventsData.Services.userServices;
using ChatAndEvents.Data.EventsData.Services.attendedEventServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace ChatAndEvents.Web.Controllers;

[Authorize]
public class MyEventsController : Controller
{
    private readonly IUserService _userService;
    private readonly IAttendedEventService _attendedEventService;

    public MyEventsController(IUserService userService, IAttendedEventService attendedEventService)
    {
        _userService = userService;
        _attendedEventService = attendedEventService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = new MyEventsViewModel();

        try
        {
            var currentUser = await _userService.GetCurrentUser();

            // 1. Call the exact method from your interface
            var attendedEvents = await _attendedEventService.GetAttendedEventsAsync(currentUser.UserId);

            // 2. Extract the actual 'Event' data out of the 'AttendedEvent' wrapper
            // (This assumes your AttendedEvent model has a property called 'Event')
            if (attendedEvents != null)
            {
                viewModel.Events = attendedEvents
                    .Where(ae => ae.Event != null) // Safety check
                    .Select(ae => ae.Event!)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }

        return View(viewModel);
    }
}