using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.interfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;
using ChatModule.src.Interfaces.Services;

namespace ChatModule.Services
{
    public class FriendListService : IFriendListService
    {
        private readonly IFriendRepository friendRepository;
        private readonly IUserRepository userRepository;

        public FriendListService(IFriendRepository friendRepository, IUserRepository userRepository)
        {
            this.friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<List<User>> GetFriendsAsync(Guid targetUserId)
        {
            var acceptedFriendList = await friendRepository.GetAcceptedFriendsAsync(targetUserId);
            var actualUserFriends = new List<User>();

            foreach (var friendRelation in acceptedFriendList)
            {
                var friendUserId = friendRelation.UserId1 == targetUserId ? friendRelation.UserId2 : friendRelation.UserId1;
                var user = await userRepository.GetByIdAsync(friendUserId);
                if (user != null)
                {
                    actualUserFriends.Add(user);
                }
            }

            return actualUserFriends;
        }

        public async Task RemoveFriendAsync(Guid currentUserId, Guid targetFriendId)
        {
            await friendRepository.DeleteFriendshipAsync(currentUserId, targetFriendId);
        }
    }
}
