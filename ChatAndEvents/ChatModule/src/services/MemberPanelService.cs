using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Repositories;
using ChatModule.src.domain.Enums;
using ChatModule.src.Interfaces.Repositories;
using ChatModule.src.Interfaces.Services;

namespace ChatModule.Services
{
    public class MemberPanelService : IMemberPanelService
    {
        private readonly IParticipantRepository _participantRepository;
        private readonly IUserRepository _userRepository;

        public MemberPanelService(
            IParticipantRepository participantRepository,
            IUserRepository userRepository)
        {
            _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<List<Participant>> GetMembersAsync(Guid conversationId)
        {
            return await _participantRepository.GetAllForConversationAsync(conversationId);
        }

        public async Task<List<Participant>> GetBannedMembersAsync(Guid conversationId)
        {
            var participants = await _participantRepository.GetAllForConversationAsync(conversationId);
            return participants.Where(participant => participant.Role == ParticipantRole.Banned).ToList();
        }

        public async Task<List<User>> SearchUsersToAddAsync(Guid conversationId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<User>();
            }

            var participants = await _participantRepository.GetAllForConversationAsync(conversationId);
            var existingUserIds = participants
                .Select(participant => participant.UserId)
                .ToHashSet();

            var users = await _userRepository.SearchByUsernameAsync(query);
            return users
                .Where(user => !existingUserIds.Contains(user.Id))
                .ToList();
        }

        public async Task<User?> GetUserAsync(Guid userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }
    }
}