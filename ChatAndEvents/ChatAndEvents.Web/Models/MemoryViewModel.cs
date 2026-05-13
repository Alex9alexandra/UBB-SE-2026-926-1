using Events_GSS.Data.Models;

namespace ChatAndEvents.Web.Models;

public class MemoryViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public List<Memory> Memories { get; set; } = new();
}