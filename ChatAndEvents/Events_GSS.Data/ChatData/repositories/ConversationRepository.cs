using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.interfaces.Repositories;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly AppDbContext _db;

        public ConversationRepository(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Conversation?> GetByIdAsync(Guid id)
        {
            return await _db.Conversations.FirstOrDefaultAsync(conversation => conversation.Id == id);
        }

        public async Task<List<Conversation>> GetAllForUserAsync(Guid userId)
        {
            return await (from conversation in _db.Conversations
                          join participant in _db.Participants on conversation.Id equals participant.ConversationId
                          where participant.UserId == userId
                          select conversation)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Conversation?> GetDmBetweenAsync(Guid userId1, Guid userId2)
        {
            return await _db.Conversations
                .Where(conversation => conversation.Type == ConversationType.Dm)
                .Where(conversation =>
                    _db.Participants
                        .Where(participant =>
                            participant.ConversationId == conversation.Id &&
                            (participant.UserId == userId1 || participant.UserId == userId2))
                        .Select(participant => participant.UserId)
                        .Distinct()
                        .Count() == 2)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Conversation c)
        {
            _db.Conversations.Add(c);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Conversation c)
        {
            var conversation = await _db.Conversations.FindAsync(c.Id);
            if (conversation == null)
            {
                return;
            }

            conversation.Title = c.Title;
            conversation.IconUrl = c.IconUrl;
            await _db.SaveChangesAsync();
        }

        public async Task SetPinnedMessageAsync(Guid conversationId, Guid? messageId)
        {
            var conversation = await _db.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return;
            }

            conversation.PinnedMessageId = messageId;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid conversationId)
        {
            var messages = await _db.Messages
                .Where(message => message.ConversationId == conversationId)
                .ToListAsync();

            var participants = await _db.Participants
                .Where(participant => participant.ConversationId == conversationId)
                .ToListAsync();

            var conversation = await _db.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return;
            }

            _db.Messages.RemoveRange(messages);
            _db.Participants.RemoveRange(participants);
            _db.Conversations.Remove(conversation);
            await _db.SaveChangesAsync();
        }
    }
}
