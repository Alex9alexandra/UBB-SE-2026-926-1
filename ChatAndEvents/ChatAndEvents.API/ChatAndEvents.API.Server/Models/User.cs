// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Events_GSS.Data.Models
{
    using ChatModule.src.domain.Enums;
    using System;

    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the reputation points of the user.
        /// </summary>
        public int ReputationPoints { get; set; } = UserDefaults.DefaultReputationPoints;
    }
}