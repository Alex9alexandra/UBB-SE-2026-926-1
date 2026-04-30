using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatModule.Services
{
    public interface IMessageInteractionService
    {
        Task ReactToMessageAsync(Guid messageId, Guid userId, string emoji);
        Task RemoveReactionAsync(Guid messageId, Guid userId);
        Task<List<Message>> GetReactionsAsync(Guid messageId);
        Task<string?> BuildReplyPreviewAsync(Guid messageId);
        Task<(string Sender, string Content)?> BuildReplyPreviewPartsAsync(Guid messageId);
    }
}