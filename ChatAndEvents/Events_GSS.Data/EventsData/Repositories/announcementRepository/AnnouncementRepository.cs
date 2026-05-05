// <copyright file="AnnouncementRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories.announcementRepository;

using System.Data;
using ChatAndEvents.Data.EventsData.Database;
using ChatAndEvents.Data.EventsData.Models;

using Microsoft.Data.SqlClient;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly AppDbContext _db;

    public AnnouncementRepository(AppDbContext db)
    {
        this._db = db;
    }

    public async Task<int> AddAnnouncementAsync(Announcement announcement, int eventId, Guid userId)
    {
        if (announcement == null)
        {
            throw new ArgumentNullException(nameof(announcement));
        }

        if (string.IsNullOrWhiteSpace(announcement.Message))
        {
            throw new ArgumentException("Message is required.", nameof(announcement));
        }

        announcement.EventId = eventId;
        announcement.AuthorId = userId;

        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();
        return announcement.AnnouncementId;
    }

    public async Task DeleteAnnouncementAsync(int selectedEvent)
    {
        var announcement = _db.Announcements.Find(selectedEvent);
        if (announcement != null)
        {
            _db.Announcements.Remove(announcement);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetAllParticipantsAsync(int eventId)
    {
        return await _db.AttendedEvents
            .Where(ae => ae.EventId == eventId)
            .Select(ae => ae.User)
            .ToListAsync();
    }

    public async Task<Announcement?> GetAnnouncementByIdAsync(int announcementId)
    {
        return await _db.Announcements
            .Include(a => a.Event)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId);
    }

    public async Task<List<Announcement>> GetAnnouncementsByEventAsync(int eventId, Guid userId)
    {
        return await _db.Announcements
            .Include(a => a.Event)
            .Include(a => a.Author)
            .Where(a => a.EventId == eventId)
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<List<(int AnnouncementId, AnnouncementReaction Reaction)>> GetReactionsAsync(List<int> announcementIds)
    {
        var reactions = await _db.AnnouncementReactions
            .Include(ar => ar.Author)
            .Where(ar => announcementIds.Contains(ar.AnnouncementId))
            .ToListAsync(); 
        return reactions
            .Select(r=>(r.AnnouncementId, r))
            .ToList();
    }

    public async Task<List<AnnouncementReadReceipt>> GetReadReceiptsAsync(int announcementId)
    {
        return await _db.AnnouncementReadReceipts
            .Where(arr => arr.AnnouncementId == announcementId)
            .ToListAsync();
    }

    public async Task<int> GetTotalParticipantsAsync(int eventId)
    {
        return await _db.AttendedEvents
            .Where(ae => ae.EventId == eventId)
            .CountAsync();
    }

    public async Task<Dictionary<int, int>> GetUnreadCountsForUserAsync(Guid userId)
    {
        return await _db.Announcements
            .Where(a => _db.AttendedEvents.Any(ae => ae.UserId == userId && ae.EventId == a.EventId))
            .Where(a => !_db.AnnouncementReadReceipts.Any(r => r.AnnouncementId == a.AnnouncementId && r.UserId == userId))
            .GroupBy(a => a.EventId)
            .Select(g => new { EventId = g.Key, UnreadCount = g.Count() })
            .ToDictionaryAsync(x => x.EventId, x => x.UnreadCount);
    }

    public async Task<string?> GetUserReactionAsync(int announcementId, Guid userId)
    {
        return await _db.AnnouncementReactions
            .Where(ar => ar.AnnouncementId == announcementId && ar.AuthorId == userId)
            .Select(ar => ar.Emoji)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasUserReadAsync(int announcementId, Guid userId)
    {
        return await _db.AnnouncementReadReceipts
            .AnyAsync(r => r.AnnouncementId == announcementId && r.UserId == userId);
    }

    public async Task InsertReactionAsync(int announcementId, Guid userId, string emoji)
    {
        var announcement = await _db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var announcementReaction = new AnnouncementReaction
        {
            AnnouncementId = announcementId,
            AuthorId = userId,
            Author = user,
            Emoji = emoji
        };

        _db.AnnouncementReactions.Add(announcementReaction);
        await _db.SaveChangesAsync();
    }

    public async Task InsertReadReceiptAsync(int announcementId, Guid userId)
    {
        var announcement = await _db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var readReceipt = new AnnouncementReadReceipt
        {
            AnnouncementId = announcementId,
            UserId = userId,
            User = user,
            ReadAt = DateTime.UtcNow,
        };

        _db.AnnouncementReadReceipts.Add(readReceipt);
        await _db.SaveChangesAsync();
    }

    public async Task PinAsync(int announcementId)
    {
        var announcement = await _db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }
        announcement.IsPinned = true;
        await _db.SaveChangesAsync();

    }

    public async Task RemoveReactionAsync(int announcementId, Guid userId)
    {
        var announcementReaction=await _db.AnnouncementReactions
            .Where(ar => ar.AnnouncementId == announcementId && ar.AuthorId == userId)
            .FirstOrDefaultAsync();
        if (announcementReaction != null)
        {
            _db.AnnouncementReactions.Remove(announcementReaction);
            await _db.SaveChangesAsync();
        }
    }

    public async Task UnpinAnnouncementAsync(int eventId)
    {
        await _db.Announcements
            .Where(a => a.EventId == eventId && a.IsPinned)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IsPinned, false));
    }

    public async Task UpdateAnnouncementAsync(int announcementId, string newMessage)
    {
        var announcement = await _db.Announcements.FindAsync(announcementId);
        if (announcement == null)
        {
            throw new InvalidOperationException("Announcement not found");
        }
        
        announcement.Message = newMessage;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateReactionAsync(int announcementId, Guid userId, string emoji)
    {
        var announcementReaction = await _db.AnnouncementReactions
            .Where(ar => ar.AnnouncementId == announcementId && ar.AuthorId == userId)
            .FirstOrDefaultAsync();
        if (announcementReaction != null)
        {
            announcementReaction.Emoji = emoji;
            await _db.SaveChangesAsync();
        }
    }
}
