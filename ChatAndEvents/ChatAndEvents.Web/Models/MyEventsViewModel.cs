using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Web.Models;

public class MyEventsViewModel
{
    public List<Event> Events { get; set; } = new();
    public bool IsEmpty => Events.Count == 0;
    public string? ErrorMessage { get; set; }
}