using ChatModule.src.domain;
using ChatModule.src.domain.Enums;
using Events_GSS.Data.Models;
using System;
using System.Collections.Generic;

namespace ChatModule.Models
{
    public class Message
    {
        // 1. Primary Key
        public Guid Id { get; set; }

        // 2. Navigation Properties (Class-based links) 
        public Conversation Conversation { get; set; } // Replaces ConversationId
        public Guid ConversationId { get; set; } // Required Foreign Key for EF

        public User Sender { get; set; } // Replaces UserId
        public Guid? SenderId { get; set; }

        public Message? ReplyTo { get; set; } // Replaces ReplyToId
        public Guid? ReplyToId { get; set; }

        // 3. Domain Data
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public MessageType MessageType { get; set; }
        
        public string? AttachmentImagePath { get; set; }

        // 4. Relationships (Replaces Dictionary)
        // You'll need a MessageReaction class to track who reacted with what
        public Dictionary<string, int> ReactionCounts { get; set; } = new ();        
        // 5. Read Receipts (Navigation property to a list of users)
        public List<User> ReadByUsers { get; set; } = new();

        // --- REMOVED: IsMine, CanEdit, SenderInitial, etc. ---
        // These belong in the ChatViewModel.cs on the client side! 
    }
}