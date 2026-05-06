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
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _db;

        public MessageRepository(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Message?> GetByIdAsync(Guid id)
        {
            return await _db.Messages.FirstOrDefaultAsync(message => message.Id == id);
        }

        public async Task<List<Message>> GetAllForConversationAsync(Guid conversationId)
        {
            return await _db.Messages
                .Where(message => message.ConversationId == conversationId)
                .OrderBy(message => message.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Message>> GetByConversationAsync(Guid conversationId, int skip, int take)
        {
            return await _db.Messages
                .Where(message => message.ConversationId == conversationId && message.MessageType != MessageType.Reaction)
                .OrderBy(message => message.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task CreateAsync(Message m)
        {
            _db.Messages.Add(m);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateContentAsync(Guid id, string newContent)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
            {
                return;
            }

            message.Content = newContent;
            await _db.SaveChangesAsync();
        }

        public async Task SetPinExpiresAtAsync(Guid id, DateTime? expiresAt)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
            {
                return;
            }

            message.PinExpiresAt = expiresAt;
            await _db.SaveChangesAsync();
        }

        public async Task<List<Message>> GetReactionsForMessageAsync(Guid parentMessageId)
        {
            return await _db.Messages
                .Where(message => message.MessageType == MessageType.Reaction && message.ParentMessageId == parentMessageId)
                .ToListAsync();
        }

        public async Task<List<Message>> SearchInConversationAsync(Guid conversationId, string query)
        {
            return await _db.Messages
                .Where(message => message.ConversationId == conversationId &&
                                  message.Content != null &&
                                  EF.Functions.Like(message.Content, $"%{query}%"))
                .OrderBy(message => message.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Message>> GetSystemMessagesAsync(Guid conversationId)
        {
            return await _db.Messages
                .Where(message => message.ConversationId == conversationId && message.MessageType == MessageType.System)
                .ToListAsync();
        }

        public async Task<Message?> GetLastMessageAsync(Guid conversationId)
        {
            return await _db.Messages
                .Where(message => message.ConversationId == conversationId && message.MessageType != MessageType.Reaction)
                .OrderByDescending(message => message.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountUnreadAsync(Guid conversationId, Guid lastReadMessageId, Guid userId)
        {
            var lastReadMessage = await _db.Messages.FirstOrDefaultAsync(message => message.Id == lastReadMessageId);
            if (lastReadMessage == null)
            {
                return 0;
            }

            return await _db.Messages.CountAsync(message =>
                message.ConversationId == conversationId &&
                message.CreatedAt > lastReadMessage.CreatedAt &&
                message.MessageType != MessageType.Reaction &&
                (message.UserId == null || message.UserId != userId));
        }

        public async Task<Guid?> GetLatestReadableMessageIdAsync(Guid conversationId, Guid userId)
        {
            return await _db.Messages
                .Where(message =>
                    message.ConversationId == conversationId &&
                    message.MessageType != MessageType.Reaction &&
                    (message.UserId == null || message.UserId != userId))
                .OrderByDescending(message => message.CreatedAt)
                .Select(message => (Guid?)message.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountUnreadFromStartAsync(Guid conversationId, Guid userId)
        {
            return await _db.Messages.CountAsync(message =>
                message.ConversationId == conversationId &&
                message.MessageType != MessageType.Reaction &&
                (message.UserId == null || message.UserId != userId));
        }

        public async Task<int> CountReadByAsync(Guid conversationId, Guid messageId)
        {
            var targetMessage = await _db.Messages.FirstOrDefaultAsync(message => message.Id == messageId);
            if (targetMessage == null)
            {
                return 0;
            }

            return await (from participant in _db.Participants
                          join lastRead in _db.Messages on participant.LastReadMessageId equals lastRead.Id
                          where participant.ConversationId == conversationId &&
                                lastRead.ConversationId == conversationId &&
                                lastRead.CreatedAt >= targetMessage.CreatedAt
                          select participant).CountAsync();
        }

        public async Task<List<Guid>> GetReadByUserIdsAsync(Guid conversationId, Guid messageId)
        {
            var targetMessage = await _db.Messages.FirstOrDefaultAsync(message => message.Id == messageId);
            if (targetMessage == null)
            {
                return new List<Guid>();
            }

            return await (from participant in _db.Participants
                          join lastRead in _db.Messages on participant.LastReadMessageId equals lastRead.Id
                          where participant.ConversationId == conversationId &&
                                lastRead.ConversationId == conversationId &&
                                lastRead.CreatedAt >= targetMessage.CreatedAt
                          orderby participant.UserId
                          select participant.UserId).ToListAsync();
        }

        public async Task SetEditedAsync(Guid id)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
            {
                return;
            }

            message.IsEdited = true;
            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
            {
                return;
            }

            message.IsDeleted = true;
            await _db.SaveChangesAsync();
        }

        public async Task UnsoftDeleteAsync(Guid id)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
            {
                return;
            }

            message.IsDeleted = false;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteByConversationAsync(Guid conversationId)
        {
            var messages = await _db.Messages
                .Where(message => message.ConversationId == conversationId)
                .ToListAsync();

            _db.Messages.RemoveRange(messages);
            await _db.SaveChangesAsync();
        }
    }
}