using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatModule.Services
{
    public interface IMentionService
    {
        Task<List<User>> GetCandidatesAsync(Guid conversationId, string query);
        Task<List<Guid>> ExtractMentionedUserIdsAsync(Guid conversationId, string content);
    }
}