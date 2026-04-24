using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatModule.src.domain;

namespace ChatModule.src.Interfaces.Services
{
    public interface IGroupService
    {
        Task<Conversation> CreateGroupAsync(Guid creatorId, string title, string? iconUrl, List<Guid> memberIds);
        Task UpdateGroupInfoAsync(Guid conversationId, Guid requesterId, string? newTitle, string? newIconUrl);
        Task LeaveGroupAsync(Guid conversationId, Guid userId);
        Task PinMessageAsync(Guid conversationId, Guid requesterId, Guid messageId);
        Task UnpinMessageAsync(Guid conversationId, Guid requesterId);
        Task PostEventNoticeAsync(Guid conversationId, Guid adminId, string eventTitle, DateTime eventDate);
    }
}