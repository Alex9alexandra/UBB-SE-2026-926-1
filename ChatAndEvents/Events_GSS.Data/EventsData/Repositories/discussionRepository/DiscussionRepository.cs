namespace ChatAndEvents.Data.EventsData.Repositories.discussionRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Events_GSS.Data.Models;
using Microsoft.EntityFrameworkCore;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly AppDbContext _db;

    public DiscussionRepository(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<DiscussionMessage>> GetByEventAsync(int eventId, Guid currentUserId)
    {
        return await _db.DiscussionMessages
            .AsNoTracking()
            .Where(d => d.AssociatedEvent!.EventId == eventId)
            .Include(d => d.Author)
            .Include(d => d.ReplyTo)
                .ThenInclude(r => r!.Author)
            .Include(d => d.Reactions)
                .ThenInclude(r => r.Author)
            .OrderBy(d => d.DateCreated)
            .ToListAsync();
    }

    public async Task<DiscussionMessage?> GetByIdAsync(int messageId)
    {
        return await _db.DiscussionMessages
            .AsNoTracking()
            .Include(d => d.Author)
            .Include(d => d.ReplyTo)
                .ThenInclude(r => r!.Author)
            .Include(d => d.Reactions)
                .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(d => d.Id == messageId);
    }

    public async Task<int> AddAsync(DiscussionMessage message)
    {
        if (message.AssociatedEvent == null || message.Author == null)
            throw new ArgumentException("Event and Author are required.", nameof(message));

        _db.DiscussionMessages.Add(message);
        await _db.SaveChangesAsync();
        return message.Id;
    }

    public async Task DeleteAsync(int messageId)
    {
        await DetachRepliesAsync(messageId);
        
        await _db.DiscussionReactions
            .Where(r => r.Message!.Id == messageId)
            .ExecuteDeleteAsync();
        
        var message = await _db.DiscussionMessages.FindAsync(messageId);
        if (message != null)
        {
            _db.DiscussionMessages.Remove(message);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<DateTime?> GetLastUserMessageDateAsync(int eventId, Guid userId)
    {
        return await _db.DiscussionMessages
            .AsNoTracking()
            .Where(d => d.AssociatedEvent!.EventId == eventId && d.Author!.UserId == userId)
            .OrderByDescending(d => d.DateCreated)
            .Select(d => (DateTime?)d.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task DetachRepliesAsync(int messageId)
    {
        await _db.DiscussionMessages
            .Where(d => d.ReplyTo!.Id == messageId)
            .ExecuteUpdateAsync(setters => 
                setters.SetProperty(d => d.ReplyTo, (DiscussionMessage?)null));
    }
    
    public async Task AddReactionAsync(int messageId, Guid userId, string emoji)
    {
        var message = await _db.DiscussionMessages.FindAsync(messageId);
        if (message == null)
            throw new InvalidOperationException("Message not found.");

        var user = new User { UserId = userId };  
        if (user == null)
            throw new InvalidOperationException("User not found.");

        var reaction = new DiscussionReaction
        {
            Message = message,
            Author = user,
            Emoji = emoji
        };

        _db.DiscussionReactions.Add(reaction);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveReactionAsync(int messageId, Guid userId)
    {
        await _db.DiscussionReactions
            .Where(r => r.Message!.Id == messageId && r.Author!.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task<DiscussionReaction?> GetReactionAsync(int messageId, Guid userId)
    {
        return await _db.DiscussionReactions
            .AsNoTracking()
            .Include(r => r.Author)
            .Include(r => r.Message)
            .FirstOrDefaultAsync(r => r.Message!.Id == messageId && r.Author!.UserId == userId);
    }

    public async Task<List<DiscussionReaction>> GetReactionsAsync(int messageId)
    {
        return await _db.DiscussionReactions
            .AsNoTracking()
            .Where(r => r.Message!.Id == messageId)
            .Include(r => r.Author)
            .ToListAsync();
    }

    public async Task UpdateReactionAsync(int messageId, Guid userId, string emoji)
    {
        await _db.DiscussionReactions
            .Where(r => r.Message!.Id == messageId && r.Author!.UserId == userId)
            .ExecuteUpdateAsync(setters => 
                setters.SetProperty(r => r.Emoji, emoji));
    }
    
    public async Task<DiscussionMute?> GetMuteAsync(int eventId, Guid userId)
    {
        return await _db.DiscussionMutes
            .AsNoTracking()
            .Include(m => m.MutedUser)
            .Include(m => m.MutedBy)
            .FirstOrDefaultAsync(m => m.DiscussionId == eventId && m.UserId == userId);
    }

    public async Task DeleteExistingMuteAsync(int eventId, Guid userId)
    {
        await _db.DiscussionMutes
            .Where(m => m.DiscussionId == eventId && m.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task InsertMuteAsync(DiscussionMute mute)
    {
        if (mute.MutedUser == null || mute.MutedBy == null)
            throw new ArgumentException("MutedUser and MutedBy are required.", nameof(mute));

        _db.DiscussionMutes.Add(mute);
        await _db.SaveChangesAsync();
    }

    public async Task UnmuteAsync(int eventId, Guid userId)
    {
        await _db.DiscussionMutes
            .Where(m => m.DiscussionId == eventId && m.UserId == userId)
            .ExecuteDeleteAsync();
    }
    
    public async Task SetSlowModeAsync(int eventId, int? seconds)
    {
        var @event = await _db.Events.FindAsync(eventId);
        if (@event != null)
        {
            @event.SlowModeSeconds = seconds;
            await _db.SaveChangesAsync();
        }
    }
    
    public async Task<List<User>> GetEventParticipantsAsync(int eventId)
    {
        return await _db.AttendedEvents
            .AsNoTracking()
            .Where(ae => ae.EventId == eventId)
            .Select(ae => ae.User)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
}
