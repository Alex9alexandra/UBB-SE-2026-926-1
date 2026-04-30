using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.interfaces.Repositories;
using ChatAndEvents.Data.ChatData.repositories;

namespace ChatModule.Services
{
    public class SearchService : ISearchService
    {
        private readonly IMessageRepository messageRepository;
        private readonly IParticipantRepository participantRepository;
        private readonly IUserRepository userRepository;

        public SearchService(
            IMessageRepository messageRepository,
            IParticipantRepository participantRepository,
            IUserRepository userRepository)
        {
            this.messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            this.participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<List<Message>> SearchMessagesAsync(Guid conversationId, Guid userId, string query)
        {
            var participant = await participantRepository.GetAsync(conversationId, userId);
            if (participant == null)
            {
                throw new InvalidOperationException("Participant not found for this conversation.");
            }

            var messages = await messageRepository.SearchInConversationAsync(conversationId, query);
            foreach (var message in messages)
            {
                if (message.UserId.HasValue)
                {
                    var sender = await userRepository.GetByIdAsync(message.UserId.Value);
                    message.SenderUsername = sender?.Username ?? "Unknown User";
                }
                else
                {
                    message.SenderUsername = "System";
                }
            }

            return messages;
        }

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<User>();
            }

            return await userRepository.SearchByUsernameAsync(query);
        }

        public async Task<List<User>> SearchUsersForAddMemberAsync(Guid conversationId, string query)
        {
            var existingParticipants = await participantRepository.GetAllForConversationAsync(conversationId);
            var existingUserIds = existingParticipants.Select(participant => participant.UserId).ToHashSet();

            var users = await userRepository.SearchByUsernameAsync(query);
            return users.Where(user => !existingUserIds.Contains(user.Id)).ToList();
        }
    }
}
