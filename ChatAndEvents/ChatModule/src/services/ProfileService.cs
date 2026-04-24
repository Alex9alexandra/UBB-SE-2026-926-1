using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Repositories;
using ChatModule.src.domain.Enums;
using ChatModule.src.Interfaces.Repositories;

namespace ChatModule.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository userRepository;
        private readonly IFriendRepository friendRepository;

        public ProfileService(IUserRepository userRepository, IFriendRepository friendRepository)
        {
            this.userRepository = userRepository;
            this.friendRepository = friendRepository;
        }

        public async Task<User?> GetProfileAsync(Guid targetUserId)
        {
            return await userRepository.GetByIdAsync(targetUserId);
        }

        public async Task<List<User>> GetAllUsersAsync(Guid viewerUserId, string? searchQuery)
        {
            var users = string.IsNullOrWhiteSpace(searchQuery)
                ? await userRepository.GetAllAsync()
                : await userRepository.SearchByUsernameAsync(searchQuery);

            return users.Where(user => user.Id != viewerUserId).ToList();
        }

        public async Task UpdateProfileAsync(Guid userId, string? bio, string? avatarUrl, DateTime? birthday)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            if (bio != null)
            {
                user.Bio = bio;
            }

            if (avatarUrl != null)
            {
                user.AvatarUrl = avatarUrl;
            }

            if (birthday != null)
            {
                user.Birthday = birthday;
            }

            await userRepository.UpdateAsync(user);
        }

        public async Task UpdateStatusAsync(Guid userId, UserStatus status)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            user.Status = status;
            await userRepository.UpdateAsync(user);
        }

        public async Task<List<User>> GetMutualFriendsAsync(Guid userId1, Guid userId2)
        {
            var mutualFriendIds = await friendRepository.GetMutualFriendIdentifiersAsync(userId1, userId2);
            var mutualFriends = new List<User>();

            foreach (var mutualFriendId in mutualFriendIds)
            {
                var user = await userRepository.GetByIdAsync(mutualFriendId);
                if (user != null)
                {
                    mutualFriends.Add(user);
                }
            }

            return mutualFriends;
        }
    }
}
