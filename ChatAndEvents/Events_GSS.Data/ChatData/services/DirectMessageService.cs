using System;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repoInterfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;

namespace ChatAndEvents.Data.ChatData.services
{
    public class DirectMessageService : IDirectMessageService
    {
        private readonly IConversationRepository conversationRepository;
        private readonly IParticipantRepository participantRepository;
        private readonly IMessageRepository messageRepository;
        private readonly BlockService blockService;
        private readonly IUserRepository userRepository;

        public DirectMessageService(
            IConversationRepository conversationRepository,
            IParticipantRepository participantRepository,
            IFriendRepository friendRepository,
            IUserRepository userRepository,
            IMessageRepository messageRepository)
        {
            this.conversationRepository = conversationRepository;
            this.participantRepository = participantRepository;
            this.userRepository = userRepository;
            this.messageRepository = messageRepository;
            blockService = new BlockService(friendRepository, userRepository);
        }

        public async Task<Conversation> GetOrCreateAsync(Guid userId1, Guid userId2)
        {
            var existingDm = await conversationRepository.GetDmBetweenAsync(userId1, userId2);
            if (existingDm != null)
            {
                return existingDm;
            }

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Dm,
                Title = null,
                IconUrl = null,
                CreatedBy = userId1,
                PinnedMessageId = null
            };

            await conversationRepository.CreateAsync(conversation);

            var now = DateTime.UtcNow;
            await participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                UserId = userId1,
                JoinedAt = now,
                Role = ParticipantRole.Member,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false
            });

            await participantRepository.CreateAsync(new Participant
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                UserId = userId2,
                JoinedAt = now,
                Role = ParticipantRole.Member,
                LastReadMessageId = null,
                TimeoutUntil = null,
                IsFavourite = false
            });

            return conversation;
        }

        public async Task<Participant?> GetOtherParticipantAsync(Guid conversationId, Guid currentUserId)
        {
            var participants = await participantRepository.GetAllForConversationAsync(conversationId);
            return participants.FirstOrDefault(participant => participant.UserId != currentUserId);
        }

        public async Task<bool> IsBlockedAsync(Guid conversationId, Guid viewerUserId)
        {
            var otherParticipant = await GetOtherParticipantAsync(conversationId, viewerUserId);
            if (otherParticipant == null)
            {
                return false;
            }

            var blockedByViewer = await blockService.IsBlockedAsync(viewerUserId, otherParticipant.UserId);
            var blockedByOther = await blockService.IsBlockedAsync(otherParticipant.UserId, viewerUserId);

            return blockedByViewer || blockedByOther;
        }

        public async Task<User?> GetOtherUserAsync(Guid conversationId, Guid viewerUserId)
        {
            var otherParticipant = await GetOtherParticipantAsync(conversationId, viewerUserId);
            if (otherParticipant == null)
            {
                return null;
            }

            return await userRepository.GetByIdAsync(otherParticipant.UserId);
        }

        public async Task<(Message Pinned, Message Notice)> PinMessageAsync(Guid conversationId, Guid requesterId, Guid messageId, DateTime expiresAt)
        {
            var participants = await participantRepository.GetAllForConversationAsync(conversationId);
            if (!participants.Any(participant => participant.UserId == requesterId))
            {
                throw new InvalidOperationException("You are not a participant in this conversation.");
            }

            var message = await messageRepository.GetByIdAsync(messageId)
                ?? throw new InvalidOperationException("Message not found.");

            if (message.ConversationId != conversationId)
            {
                throw new InvalidOperationException("Message does not belong to this conversation.");
            }

            // Clear PinExpiresAt on any previously pinned message
            var conversation = await conversationRepository.GetByIdAsync(conversationId);
            if (conversation?.PinnedMessageId != null && conversation.PinnedMessageId != messageId)
            {
                await messageRepository.SetPinExpiresAtAsync(conversation.PinnedMessageId.Value, null);
            }

            await conversationRepository.SetPinnedMessageAsync(conversationId, messageId);
            await messageRepository.SetPinExpiresAtAsync(messageId, expiresAt);
            message.PinExpiresAt = expiresAt;

            var user = await userRepository.GetByIdAsync(requesterId);
            var username = user?.Username ?? "Someone";
            var notice = await WriteSystemMessageAsync(conversationId, $"{username} pinned a message.");

            return (message, notice);
        }

        public async Task<Message> UnpinMessageAsync(Guid conversationId, Guid requesterId)
        {
            var participants = await participantRepository.GetAllForConversationAsync(conversationId);
            if (!participants.Any(p => p.UserId == requesterId))
            {
                throw new InvalidOperationException("You are not a participant in this conversation.");
            }

            var conversation = await conversationRepository.GetByIdAsync(conversationId);
            if (conversation?.PinnedMessageId != null)
            {
                await messageRepository.SetPinExpiresAtAsync(conversation.PinnedMessageId.Value, null);
            }

            await conversationRepository.SetPinnedMessageAsync(conversationId, null);

            var user = await userRepository.GetByIdAsync(requesterId);
            var username = user?.Username ?? "Someone";
            return await WriteSystemMessageAsync(conversationId, $"{username} unpinned a message.");
        }

        public async Task ClearExpiredPinAsync(Guid conversationId, Guid pinnedMessageId)
        {
            await messageRepository.SetPinExpiresAtAsync(pinnedMessageId, null);
            await conversationRepository.SetPinnedMessageAsync(conversationId, null);
        }

        private async Task<Message> WriteSystemMessageAsync(Guid conversationId, string text)
        {
            var notice = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                UserId = null,
                Content = text,
                CreatedAt = DateTime.UtcNow,
                ReplyToId = null,
                IsEdited = false,
                IsDeleted = false,
                MessageType = MessageType.System,
                ParentMessageId = null
            };
            await messageRepository.CreateAsync(notice);
            return notice;
        }
    }
}
