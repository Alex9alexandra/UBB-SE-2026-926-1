using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatModule.Models;

namespace ChatModule.src.Interfaces.Services
{
    public interface IMemberPanelService
    {
        Task<List<Participant>> GetMembersAsync(Guid conversationId);
        Task<List<Participant>> GetBannedMembersAsync(Guid conversationId);
        Task<List<User>> SearchUsersToAddAsync(Guid conversationId, string query);
        Task<User?> GetUserAsync(Guid userId);
    }
}