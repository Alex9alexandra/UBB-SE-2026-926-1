using ChatAndEvents.Data.EventsData.Models;
using Events_GSS.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatAndEvents.Data.EventsData.Models;
public class DiscussionReaction
{
    public int Id { get; set; }
    public required string Emoji { get; set; }
    public required DiscussionMessage Message { get; set; }
    public required User Author { get; set; }
    public int DiscussionId { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Discussion Discussion { get; internal set; }
}
