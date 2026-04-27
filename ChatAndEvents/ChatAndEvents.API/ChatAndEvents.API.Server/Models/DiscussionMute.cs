using System;
using System.Collections.Generic;
using System.Text;

namespace Events_GSS.Data.Models;

public class DiscussionMute
{
    // 1. Primary Key (Recommend Guid to match Chat models)
    public Guid Id { get; set; }

    // 2. Navigation Properties (Class-based links)
    // Replaces 'int EventId' to satisfy the "Property of type B" rule
    public Event Event { get; set; } = null!;
    public Guid EventId { get; set; } // Foreign Key for EF Core

    // These were already good, but ensure User uses Guid Id
    public User MutedUser { get; set; } = null!;
    public Guid MutedUserId { get; set; }

    public User MutedBy { get; set; } = null!;
    public Guid MutedById { get; set; }

    // 3. Domain Data
    public DateTime? MutedUntil { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime CreatedAt { get; set; }
}