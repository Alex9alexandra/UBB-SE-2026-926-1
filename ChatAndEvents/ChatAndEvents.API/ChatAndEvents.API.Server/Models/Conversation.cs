using ChatModule.Models;
using ChatModule.src.domain.Enums;
using Events_GSS.Data.Models;
using System;
using System.Collections.Generic;

namespace ChatModule.src.domain
{
    public class Conversation
    {
        // 1. Primary Key
        public Guid Id { get; set; }

        // 2. Navigation Properties (Class-based links) 
        public User Creator { get; set; } 
        public Guid CreatorId { get; set; } // Required Foreign Key for EF Core [cite: 47]

        public Message? PinnedMessage { get; set; } 
        public Guid? PinnedMessageId { get; set; } // Required Foreign Key for EF Core [cite: 47]

        // 3. Domain Data
        public ConversationType Type { get; set; }
        public string? Title { get; set; }
        public string? IconUrl { get; set; }
        public DateTime? LastMessageAt { get; set; }

        // 4. Collections (The "Other Half" of the relationships) 
        // This allows EF Core to handle the "unified structure" [cite: 37]
        public List<Participant> Participants { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        // --- REMOVED: UnreadCount, HasUnread, LastMessagePreview ---
        // These are UI concerns and belong in your WinUI 3 ViewModels.
    }
}