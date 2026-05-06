using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.interfaces.Repositories;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public class ParticipantRepository : IParticipantRepository
    {
        private readonly AppDbContext _db;

        public ParticipantRepository(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Participant?> GetAsync(Guid conversationId, Guid userId)
        {
            return await _db.Participants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        }

        public async Task<List<Participant>> GetAllForConversationAsync(Guid conversationId)
        {
            return await _db.Participants
                .Where(p => p.ConversationId == conversationId)
                .ToListAsync();
        }

        public async Task<List<Participant>> GetAllForUserAsync(Guid userId)
        {
            return await _db.Participants
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task CreateAsync(Participant participant)
        {
            _db.Participants.Add(participant);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(Guid conversationId, Guid userId, ParticipantRole role)
        {
            var participant = await GetAsync(conversationId, userId);
            if (participant == null) return;
            participant.Role = role;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateLastReadAsync(Guid conversationId, Guid userId, Guid messageId)
        {
            var participant = await GetAsync(conversationId, userId);
            if (participant == null) return;
            participant.LastReadMessageId = messageId;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateTimeoutAsync(Guid conversationId, Guid userId, DateTime? until)
        {
            var participant = await GetAsync(conversationId, userId);
            if (participant == null) return;
            participant.TimeoutUntil = until;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateFavouriteAsync(Guid conversationId, Guid userId, bool isFav)
        {
            var participant = await GetAsync(conversationId, userId);
            if (participant == null) return;
            participant.IsFavourite = isFav;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid conversationId, Guid userId)
        {
            var participant = await GetAsync(conversationId, userId);
            if (participant == null) return;
            _db.Participants.Remove(participant);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateNicknameAsync(Guid conversationId, Guid userId, string? nickname)
        {
            var participant = await GetAsync(conversationId, userId);
            if (participant == null) return;
            participant.Nickname = nickname;
            await _db.SaveChangesAsync();
        }
    }
}
