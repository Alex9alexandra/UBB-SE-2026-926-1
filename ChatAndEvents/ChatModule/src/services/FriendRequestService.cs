using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Repositories;
using ChatModule.src.domain;
using ChatModule.src.domain.Enums;
using ChatModule.src.Interfaces.Repositories;
using ChatModule.src.Interfaces.Services;

namespace ChatModule.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IFriendRepository friendRepository;
        private readonly IUserRepository userRepository;
        private readonly IConversationRepository conversationRepository;
        private readonly IParticipantRepository participantRepository;

        public FriendRequestService(
            IFriendRepository friendRepository,
            IUserRepository userRepository,
            IConversationRepository conversationRepository,
            IParticipantRepository participantRepository)
        {
            this.friendRepository = friendRepository;
            this.userRepository = userRepository;
            this.conversationRepository = conversationRepository;
            this.participantRepository = participantRepository;
        }

        public async Task SendFriendRequestAsync(Guid senderUserId, Guid receiverUserId)
        {
            if (senderUserId == receiverUserId)
            {
                throw new InvalidOperationException("You cannot send a friend request to yourself.");
            }

            var areAlreadyFriends = await friendRepository.CheckIfFriendsAsync(senderUserId, receiverUserId);
            if (areAlreadyFriends)
            {
                throw new InvalidOperationException("Users are already friends.");
            }

            var existingFriendshipRelation = await friendRepository.GetFriendshipAsync(senderUserId, receiverUserId);
            if (existingFriendshipRelation != null)
            {
                if (existingFriendshipRelation.Status == FriendStatus.Blocked)
                {
                    await friendRepository.UpdateFriendshipStatusAsync(senderUserId, receiverUserId, FriendStatus.Pending);
                    await friendRepository.SetMatchStatusAsync(senderUserId, receiverUserId, false);
                    return;
                }

                throw new InvalidOperationException("A friend request already exists between these users.");
            }

            var newFriendRequest = new Friend
            {
                Id = Guid.NewGuid(),
                UserId1 = senderUserId,
                UserId2 = receiverUserId,
                Status = FriendStatus.Pending,
                IsMatch = false,
                CreatedAt = DateTime.UtcNow
            };

            await friendRepository.CreateFriendshipAsync(newFriendRequest);
        }

        public async Task<bool> SendFriendRequestByUsernameAsync(Guid senderUserId, string receiverUsername)
        {
            if (string.IsNullOrWhiteSpace(receiverUsername))
            {
                throw new InvalidOperationException("Please enter a username.");
            }

            var receiverUserObject = await userRepository.GetByUsernameAsync(receiverUsername.Trim());
            if (receiverUserObject == null)
            {
                return false;
            }

            await SendFriendRequestAsync(senderUserId, receiverUserObject.Id);
            return true;
        }

        public async Task AcceptFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            await friendRepository.UpdateFriendshipStatusAsync(requesterUserId, currentUserId, FriendStatus.Accepted);

            var existingConversation = await conversationRepository.GetDmBetweenAsync(currentUserId, requesterUserId);
            if (existingConversation != null)
            {
                return;
            }

            var newConversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Dm,
                Title = null,
                IconUrl = null,
                CreatedBy = currentUserId,
                PinnedMessageId = null
            };

            await conversationRepository.CreateAsync(newConversation);

            var currentDataTime = DateTime.UtcNow;

            await participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = newConversation.Id,
                UserId = currentUserId,
                JoinedAt = currentDataTime,
                Role = ParticipantRole.Member,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false
            });

            await participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = newConversation.Id,
                UserId = requesterUserId,
                JoinedAt = currentDataTime,
                Role = ParticipantRole.Member,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false
            });
        }

        public async Task DeclineFriendRequestAsync(Guid currentUserId, Guid requesterUserId)
        {
            var friendshipRelation = await friendRepository.GetFriendshipAsync(requesterUserId, currentUserId);

            if (friendshipRelation == null || friendshipRelation.Status != FriendStatus.Pending)
            {
                throw new InvalidOperationException("No pending friend request found.");
            }

            await friendRepository.DeleteFriendshipAsync(requesterUserId, currentUserId);
        }

        public async Task<List<User>> GetIncomingRequestsAsync(Guid currentUserId)
        {
            var pendingRequestList = await friendRepository.GetPendingRequestsForUserAsync(currentUserId);
            var senderUserList = new List<User>();

            foreach (var requestRelation in pendingRequestList)
            {
                var senderUserObject = await userRepository.GetByIdAsync(requestRelation.UserId1);
                if (senderUserObject != null)
                {
                    senderUserList.Add(senderUserObject);
                }
            }

            return senderUserList;
        }

        public async Task<FriendStatus?> GetRelationshipStatusAsync(Guid firstUserId, Guid secondUserId)
        {
            var friendshipRelation = await friendRepository.GetFriendshipAsync(firstUserId, secondUserId);
            return friendshipRelation?.Status;
        }
    }
}
