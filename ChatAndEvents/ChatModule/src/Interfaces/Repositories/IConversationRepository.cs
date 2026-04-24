using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatModule.src.domain;

namespace ChatModule.src.Interfaces.Repositories
{
    public interface IConversationRepository
    {
        Task CreateAsync(Conversation c);
        Task DeleteAsync(Guid conversationId);
        Task<List<Conversation>> GetAllForUserAsync(Guid userId);
        Task<Conversation?> GetByIdAsync(Guid id);
        Task<Conversation?> GetDmBetweenAsync(Guid userId1, Guid userId2);
        Task SetPinnedMessageAsync(Guid conversationId, Guid? messageId);
        Task UpdateAsync(Conversation c);
    }
}