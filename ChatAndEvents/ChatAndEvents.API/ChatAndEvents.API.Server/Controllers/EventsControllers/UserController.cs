using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Services.userServices;
using Microsoft.AspNetCore.Mvc;

namespace ChatAndEvents.API.Server.Controllers.Events;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUser();
        return Ok(user);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _userService.GetUserById(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("{userId}/friends")]
    public IActionResult GetFriends(Guid userId)
    {
        var friends = _userService.GetFriends(userId);
        return Ok(friends);
    }

    [HttpGet("{userId}/search-friends")]
    public IActionResult SearchFriends(
        Guid userId,
        [FromQuery] string name)
    {
        var friends = _userService.SearchFriends(userId, name);
        return Ok(friends);
    }

    [HttpGet("{eventId}/attending")]
    public async Task<IActionResult> IsAttending(
        int eventId)
    {
        var ev = new Event { EventId = eventId };

        var result = await _userService.IsAttending(ev);

        return Ok(result);
    }

    [HttpGet("{eventId}/admin")]
    public IActionResult IsAdmin(
        int eventId)
    {
        var ev = new Event { EventId = eventId };

        var result = _userService.IsAdmin(ev);

        return Ok(result);
    }
}