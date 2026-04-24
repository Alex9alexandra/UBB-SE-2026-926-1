using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Repositories;
using ChatModule.src.domain.Enums;
using ChatModule.src.Interfaces.Repositories;
using ChatModule.src.Interfaces.Services;

namespace ChatModule.Services
{
    public class BlockService : IBlockService
    {
        private readonly IFriendRepository friendRepository;
        private readonly IUserRepository userRepository;

        public BlockService(IFriendRepository friendRepository, IUserRepository userRepository)
        {
            this.friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task BlockUserAsync(Guid blockerUserId, Guid targetUserId)
        {
            var friendshipRelation = await friendRepository.GetFriendshipAsync(blockerUserId, targetUserId);
            if (friendshipRelation == null)
            {
                var newBlockedRelation = new Friend
                {
                    Id = Guid.NewGuid(),
                    UserId1 = blockerUserId,
                    UserId2 = targetUserId,
                    Status = FriendStatus.Blocked,
                    IsMatch = false,
                    CreatedAt = DateTime.UtcNow
                };

                await friendRepository.CreateFriendshipAsync(newBlockedRelation);

                return;
            }

            if (friendshipRelation.Status == FriendStatus.Accepted)
            {
                await friendRepository.SetMatchStatusAsync(blockerUserId, targetUserId, true);
            }
            else if (friendshipRelation.Status == FriendStatus.Pending)
            {
                await friendRepository.SetMatchStatusAsync(blockerUserId, targetUserId, false);
            }

            await friendRepository.UpdateFriendshipStatusAsync(blockerUserId, targetUserId, FriendStatus.Blocked);
        }

        public async Task UnblockUserAsync(Guid blockerUserId, Guid targetUserId)
        {
            var friendshipRelation = await friendRepository.GetFriendshipAsync(blockerUserId, targetUserId);
            if (friendshipRelation == null)
            {
                return;
            }

            if (friendshipRelation.Status == FriendStatus.Blocked)
            {
                var restoredFriendStatus = friendshipRelation.IsMatch ? FriendStatus.Accepted : FriendStatus.Pending;
                await friendRepository.UpdateFriendshipStatusAsync(blockerUserId, targetUserId, restoredFriendStatus);
                return;
            }

            await friendRepository.DeleteFriendshipAsync(blockerUserId, targetUserId);
        }

        public async Task<List<User>> GetBlockedUsersAsync(Guid targetUserId)
        {
            var blockedUserList = new List<User>();
            var allFriendshipRelations = await friendRepository.GetAllFriendshipsForUserAsync(targetUserId);

            foreach (var friendshipRelation in allFriendshipRelations)
            {
                if (friendshipRelation.Status != FriendStatus.Blocked)
                {
                    continue;
                }

                var otherUserIdentifier = friendshipRelation.UserId1 == targetUserId ? friendshipRelation.UserId2 : friendshipRelation.UserId1;
                var userObject = await userRepository.GetByIdAsync(otherUserIdentifier);

                if (userObject != null)
                {
                    blockedUserList.Add(userObject);
                }
            }

            return blockedUserList;
        }

        public async Task<bool> IsBlockedAsync(Guid blockerId, Guid targetId)
        {
            var relation = await friendRepository.GetFriendshipAsync(blockerId, targetId);
            return relation != null && relation.Status == FriendStatus.Blocked;
        }

        public async Task<bool> CheckIfBlockedAsync(Guid blockerUserId, Guid targetUserId)
        {
            var friendshipRelation = await friendRepository.GetFriendshipAsync(blockerUserId, targetUserId);
            return friendshipRelation != null && friendshipRelation.Status == FriendStatus.Blocked;
        }
    }
}
