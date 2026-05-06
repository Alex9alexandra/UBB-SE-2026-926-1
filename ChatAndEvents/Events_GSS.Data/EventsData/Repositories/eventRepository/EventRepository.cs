namespace ChatAndEvents.Data.EventsData.Repositories.eventRepository;

using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Event>> GetAllPublicActiveAsync()
    {
        return await _db.Events
            .Include(e => e.Category)
            .Include(e => e.Admin)
            .Where(e => e.IsPublic && e.EndDateTime > DateTime.UtcNow)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int eventId)
    {
        return await _db.Events
            .Include(e => e.Category)
            .Include(e => e.Admin)
            .FirstOrDefaultAsync(e => e.EventId == eventId);
    }

    public async Task<int> AddAsync(Event eventEntity)
    {
        if (eventEntity.Admin == null)
            throw new ArgumentException("Admin is required.", nameof(eventEntity));

        eventEntity.EnrolledCount = 0;

        if (eventEntity.Category != null)
        {
            eventEntity.CategoryId = eventEntity.Category.CategoryId;
            _db.Entry(eventEntity.Category).State = EntityState.Unchanged;
        }

        if (eventEntity.Admin != null)
        {
            eventEntity.AdminId = eventEntity.Admin.UserId;
            _db.Entry(eventEntity.Admin).State = EntityState.Unchanged;
        }

        _db.Events.Add(eventEntity);
        await _db.SaveChangesAsync();
        return eventEntity.EventId;
    }

    public async Task IncrementEnrolledCountAsync(int eventId)
    {
        var eventEntity = await _db.Events.FindAsync(eventId);
        if (eventEntity != null)
        {
            eventEntity.EnrolledCount++;
            await _db.SaveChangesAsync();
        }
    }

    public async Task DecrementEnrolledCountAsync(int eventId)
    {
        var eventEntity = await _db.Events.FindAsync(eventId);
        if (eventEntity != null && eventEntity.EnrolledCount > 0)
        {
            eventEntity.EnrolledCount--;
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(Event eventEntity)
    {
        _db.Events.Update(eventEntity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int eventId)
    {
        // Handle children manually since we use NoAction on delete

        var announcementIds = await _db.Set<Announcement>()
            .Where(a => a.EventId == eventId)
            .Select(a => a.AnnouncementId)
            .ToListAsync();

        await _db.Set<AnnouncementReadReceipt>()
            .Where(r => announcementIds.Contains(r.AnnouncementId))
            .ExecuteDeleteAsync();

        await _db.Set<AnnouncementReaction>()
            .Where(r => announcementIds.Contains(r.AnnouncementId))
            .ExecuteDeleteAsync();

        await _db.Set<Announcement>()
            .Where(a => a.EventId == eventId)
            .ExecuteDeleteAsync();

        var memoryIds = await _db.Memories
            .Where(m => m.EventId == eventId)
            .Select(m => m.MemoryId)
            .ToListAsync();

        await _db.Set<MemoryLike>()
            .Where(ml => memoryIds.Contains(ml.MemoryId))
            .ExecuteDeleteAsync();

        await _db.Set<QuestMemory>()
            .Where(qm => memoryIds.Contains(qm.MemoryId))
            .ExecuteDeleteAsync();

        await _db.Memories
            .Where(m => m.EventId == eventId)
            .ExecuteDeleteAsync();

        await _db.AttendedEvents
            .Where(ae => ae.EventId == eventId)
            .ExecuteDeleteAsync();

        await _db.Set<Quest>()
            .Where(q => q.EventId == eventId)
            .ExecuteDeleteAsync();

        var eventEntity = await _db.Events.FindAsync(eventId);
        if (eventEntity != null)
        {
            _db.Events.Remove(eventEntity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<Event>> GetByAdminIdAsync(Guid adminId)
    {
        return await _db.Events
            .Include(e => e.Category)
            .Include(e => e.Admin)
            .Where(e => e.AdminId == adminId)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }
}