using ChatAndEvents.Web.Models; 
using Events_GSS.Data.Models;
using Events_GSS.Data.Services.Interfaces;
using Events_GSS.Data.Services.eventServices;
using Events_GSS.Data.Services.userServices;
using Microsoft.AspNetCore.Mvc;


namespace ChatAndEvents.Web.Controllers;

public class MemoryController : Controller
{
    private readonly IMemoryService _memoryService;
    private readonly IEventService _eventService;
    private readonly IUserService _userService;

    public MemoryController(IMemoryService memoryService, IEventService eventService, IUserService userService)
    {
        _memoryService = memoryService;
        _eventService = eventService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(int eventId)
    {
        var currentUser = await _userService.GetCurrentUser();

        var currentEvent = await _eventService.GetEventByIdAsync(eventId);

        if (currentEvent == null) return NotFound();

        var memories = await _memoryService.GetByEventAsync(currentEvent, currentUser);

        var viewModel = new MemoryViewModel
        {
            EventId = eventId,
            EventName = currentEvent.Name,
            Memories = memories
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create(int eventId)
    {
        var model = new Memory { EventId = eventId }; 
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Memory memory)
    {
        var currentUser = await _userService.GetCurrentUser();
        memory.AuthorId = currentUser.UserId;
        memory.CreatedAt = DateTime.Now;

        var currentEvent = await _eventService.GetEventByIdAsync(memory.EventId);
        if (currentEvent == null) return NotFound();

        await _memoryService.AddAsync(currentEvent, currentUser, memory.PhotoPath, memory.Text);

        return RedirectToAction("Index", new { eventId = memory.EventId });
    }
}