using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChatModule.Models;
using ChatModule.Repositories;
using ChatModule.src.Interfaces.Repositories;


namespace ChatModule.Services
{
    public class MentionService : IMentionService
    {
        private const string MentionRegexPattern = "@([A-Za-z0-9_.-]+)";

        private readonly IParticipantRepository _participantRepository;
        private readonly IUserRepository _userRepository;

        public MentionService(IParticipantRepository participantRepository, IUserRepository userRepository)
        {
            this._participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
            this._userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<List<User>> GetCandidatesAsync(Guid conversationId, string query)
        {
            var participants = await this._participantRepository.GetAllForConversationAsync(conversationId);
            var memberIds = participants.Select(participant => participant.UserId).ToHashSet();

            var matchingUsers = await this._userRepository.SearchByUsernameAsync(query);
            return matchingUsers.Where(user => memberIds.Contains(user.Id)).ToList();
        }

        public async Task<List<Guid>> ExtractMentionedUserIdsAsync(Guid conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new List<Guid>();
            }

            var participants = await this._participantRepository.GetAllForConversationAsync(conversationId);
            var memberIds = participants.Select(participant => participant.UserId).ToHashSet();

            var usernames = Regex.Matches(content, MentionRegexPattern)
                .Select(match => match.Groups[1].Value)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var mentionedUserIds = new HashSet<Guid>();
            foreach (var username in usernames)
            {
                var user = await this._userRepository.GetByUsernameAsync(username);
                if (user != null && memberIds.Contains(user.Id))
                {
                    mentionedUserIds.Add(user.Id);
                }
            }

            return mentionedUserIds.ToList();
        }
    }
}