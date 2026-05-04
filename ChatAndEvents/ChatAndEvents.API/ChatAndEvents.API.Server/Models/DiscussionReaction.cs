using System;
using System.Collections.Generic;
using System.Text;
//using ChatAndEvents.Data.ChatData.domain;

namespace Events_GSS.Data.Models;
public class DiscussionReaction
{
    public int Id { get; set; }
    public required string Emoji { get; set; }
    public required DiscussionMessage Message { get; set; }
    public required User Author { get; set; }
}
