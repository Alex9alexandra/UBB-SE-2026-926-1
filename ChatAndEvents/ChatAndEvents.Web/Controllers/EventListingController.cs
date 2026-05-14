using ChatAndEvents.Data.EventsData.Services.eventServices;
using ChatAndEvents.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.Web.Controllers;

public class EventListingController : Controller
{
    private readonly IEventService _eventService;

    public EventListingController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchQuery, string? locationFilter)
    {
        var viewModel = new EventListingViewModel
        {
            SearchQuery = searchQuery,
            LocationFilter = locationFilter
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                viewModel.Events = await _eventService.SearchByTitleAsync(searchQuery);
            }
            else if (!string.IsNullOrWhiteSpace(locationFilter))
            {
                viewModel.Events = await _eventService.FilterByLocationAsync(locationFilter);
            }
            else
            {
                viewModel.Events = await _eventService.GetAllPublicActiveEventsAsync();
            }
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }

        return View(viewModel);
    }
}
