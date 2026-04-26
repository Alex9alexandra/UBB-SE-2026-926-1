using ChatModule.src.domain;
using ChatModule.src.domain.Enums;
using Events_GSS.Data.Models;
using System;

namespace ChatModule.Models
{
    public class Participant
    {
        // 1. Primary Key
        public Guid Id { get; set; }

        // 2. Navigation Properties (Class-based links) 
        public Conversation Conversation { get; set; } 
        public Guid ConversationId { get; set; } // Required Foreign Key for EF Core

        public User User { get; set; } 
        public Guid UserId { get; set; } // Required Foreign Key for EF Core

        public Message? LastReadMessage { get; set; } 
        public Guid? LastReadMessageId { get; set; } // Required Foreign Key for EF Core

        // 3. Domain Data
        public DateTime JoinedAt { get; set; }
        public ParticipantRole Role { get; set; }
        public DateTime? TimeoutUntil { get; set; }
        public bool IsFavourite { get; set; }
        public string? Nickname { get; set; }
        
        // Note: 'IsNew' is removed as it's typically UI/Business logic 
    }
}