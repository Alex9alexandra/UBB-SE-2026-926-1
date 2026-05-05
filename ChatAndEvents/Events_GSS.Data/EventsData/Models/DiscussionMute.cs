using Events_GSS.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatAndEvents.Data.EventsData.Models;

public class DiscussionMute
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public User MutedUser { get; set; } = null!;
    public User MutedBy { get; set; } = null!;

    /// <summary>
    /// Null means permanent mute.
    /// </summary>
    public DateTime? MutedUntil { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; }
    public int DiscussionId { get; set; }
    public Guid UserId {  get; set; }
    public Discussion Discussion { get; internal set; }
}

