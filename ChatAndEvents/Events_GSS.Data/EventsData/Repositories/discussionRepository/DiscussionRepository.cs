namespace ChatAndEvents.Data.EventsData.Repositories.discussionRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DiscussionRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task<List<DiscussionMessage>> GetByEventAsync(int eventId, Guid currentUserId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DiscussionMessages
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
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DiscussionMessages
            .AsNoTracking()
            .Include(d => d.Author)
            .Include(d => d.ReplyTo)
                .ThenInclude(r => r!.Author)
            .Include(d => d.Reactions)
                .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(d => d.Id == messageId);
    }

    public async Task<int> AddAsync(DiscussionMessage message, int eventId, Guid userId, int? replyToId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();

        message.AssociatedEvent = await db.Events.FindAsync(eventId)
            ?? throw new ArgumentException("Event not found.");

        // Find the user through AttendedEvents which uses EventsData.Models.User
        message.Author = await db.AttendedEvents
            .Where(ae => ae.EventId == eventId && ae.User.UserId == userId)
            .Select(ae => ae.User)
            .FirstOrDefaultAsync()
            ?? throw new ArgumentException("User not found as participant.");

        if (replyToId.HasValue)
            message.ReplyTo = await db.DiscussionMessages.FindAsync(replyToId.Value);

        db.DiscussionMessages.Add(message);
        await db.SaveChangesAsync();
        return message.Id;
    }

    public async Task DeleteAsync(int messageId)
    {
        await DetachRepliesAsync(messageId);
        
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.DiscussionReactions
            .Where(r => r.Message!.Id == messageId)
            .ExecuteDeleteAsync();
        
        var message = await db.DiscussionMessages.FindAsync(messageId);
        if (message != null)
        {
            db.DiscussionMessages.Remove(message);
            await db.SaveChangesAsync();
        }
    }

    public async Task<DateTime?> GetLastUserMessageDateAsync(int eventId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DiscussionMessages
            .AsNoTracking()
            .Where(d => d.AssociatedEvent!.EventId == eventId && d.Author!.UserId == userId)
            .OrderByDescending(d => d.DateCreated)
            .Select(d => (DateTime?)d.DateCreated)
            .FirstOrDefaultAsync();
    }

    public async Task DetachRepliesAsync(int messageId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.DiscussionMessages
            .Where(d => d.ReplyTo!.Id == messageId)
            .ExecuteUpdateAsync(setters => 
                setters.SetProperty(d => d.ReplyTo, (DiscussionMessage?)null));
    }
    
    public async Task AddReactionAsync(int messageId, Guid userId, string emoji)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var message = await db.DiscussionMessages.FindAsync(messageId);
        if (message == null)
            throw new InvalidOperationException("Message not found.");


        var reaction = new DiscussionReaction
        {
            MessageId = messageId,
            AuthorId = userId,
            Emoji = emoji
        };

        db.DiscussionReactions.Add(reaction);
        await db.SaveChangesAsync();
    }

    public async Task RemoveReactionAsync(int messageId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.DiscussionReactions
            .Where(r => r.Message!.Id == messageId && r.Author!.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task<DiscussionReaction?> GetReactionAsync(int messageId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DiscussionReactions
            .AsNoTracking()
            .Include(r => r.Author)
            .Include(r => r.Message)
            .FirstOrDefaultAsync(r => r.Message!.Id == messageId && r.Author!.UserId == userId);
    }

    public async Task<List<DiscussionReaction>> GetReactionsAsync(int messageId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DiscussionReactions
            .AsNoTracking()
            .Where(r => r.Message!.Id == messageId)
            .Include(r => r.Author)
            .ToListAsync();
    }

    public async Task UpdateReactionAsync(int messageId, Guid userId, string emoji)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.DiscussionReactions
            .Where(r => r.Message!.Id == messageId && r.Author!.UserId == userId)
            .ExecuteUpdateAsync(setters => 
                setters.SetProperty(r => r.Emoji, emoji));
    }
    
    public async Task<DiscussionMute?> GetMuteAsync(int eventId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.DiscussionMutes
            .AsNoTracking()
            .Include(m => m.MutedUser)
            .Include(m => m.MutedBy)
            .FirstOrDefaultAsync(m => m.DiscussionId == eventId && m.UserId == userId);
    }

    public async Task DeleteExistingMuteAsync(int eventId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.DiscussionMutes
            .Where(m => m.DiscussionId == eventId && m.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task InsertMuteAsync(DiscussionMute mute)
    {
        if (mute.MutedUser == null || mute.MutedBy == null)
            throw new ArgumentException("MutedUser and MutedBy are required.", nameof(mute));

        using var db = await _contextFactory.CreateDbContextAsync();
        db.DiscussionMutes.Add(mute);
        await db.SaveChangesAsync();
    }

    public async Task UnmuteAsync(int eventId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        await db.DiscussionMutes
            .Where(m => m.DiscussionId == eventId && m.UserId == userId)
            .ExecuteDeleteAsync();
    }
    
    public async Task SetSlowModeAsync(int eventId, int? seconds)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var @event = await db.Events.FindAsync(eventId);
        if (@event != null)
        {
            @event.SlowModeSeconds = seconds;
            await db.SaveChangesAsync();
        }
    }
    
    public async Task<List<User>> GetEventParticipantsAsync(int eventId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.AttendedEvents
            .AsNoTracking()
            .Where(ae => ae.EventId == eventId)
            .Select(ae => ae.User)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }
}
