using ChatModule.src.domain.Enums;
using Events_GSS.Data.Models;
using System;

namespace ChatModule.Models
{
    public class Friend
    {
        public Guid Id { get; set; }

        // --- THE CORRECT WAY (Class-based links) ---
        
        // Instead of UserId1, use the actual User object
        public User User1 { get; set; } 
        public Guid User1Id { get; set; } // Foreign Key for EF Core

        // Instead of UserId2, use the actual User object
        public User User2 { get; set; } 
        public Guid User2Id { get; set; } // Foreign Key for EF Core

        // --- METADATA ---
        public FriendStatus Status { get; set; }
        public bool IsMatch { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}