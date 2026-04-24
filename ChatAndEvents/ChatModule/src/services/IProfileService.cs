using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.src.domain.Enums;


namespace ChatModule.Services
{
    public interface IProfileService
    {
        Task<List<User>> GetAllUsersAsync(Guid viewerUserId, string? searchQuery);
        Task<List<User>> GetMutualFriendsAsync(Guid userId1, Guid userId2);
        Task<User?> GetProfileAsync(Guid targetUserId);
        Task UpdateProfileAsync(Guid userId, string? bio, string? avatarUrl, DateTime? birthday);
        Task UpdateStatusAsync(Guid userId, UserStatus status);
    }
}