using System.Data;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories
{
    public class AttendedEventRepository : IAttendedEventRepository
    {
        private readonly AppDbContext _db;

        public AttendedEventRepository(AppDbContext db)
        {
            this._db = db;
        }

        public async Task AddAsync(AttendedEvent attendedEventEntity)
        {
            if(attendedEventEntity.Event == null || attendedEventEntity.User == null)
            {
                throw new ArgumentException("Event and User are required.", nameof(attendedEventEntity));
            }
            _db.AttendedEvents.Add(attendedEventEntity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int eventId, Guid userId)
        {
            var attendedEvent = await _db.AttendedEvents.FindAsync(eventId, userId);
            if (attendedEvent != null)
            {
                _db.AttendedEvents.Remove(attendedEvent);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateIsArchivedAsync(int eventId, Guid userId, bool isArchived)
        {
            var attendedEvent = await _db.AttendedEvents.FindAsync(eventId, userId);
            if (attendedEvent != null)
            {
                attendedEvent.IsArchived = isArchived;
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateIsFavouriteAsync(int eventId, Guid userId, bool isFavourite)
        {
            var attendedEvent = await _db.AttendedEvents.FindAsync(eventId, userId);
            if (attendedEvent != null)
            {
                attendedEvent.IsFavourite = isFavourite;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<AttendedEvent?> GetAsync(int eventId, Guid userId)
        {
            return await _db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EventId == eventId && e.UserId == userId);
        }

        public async Task<List<AttendedEvent>> GetByUserIdAsync(Guid userId)
        {
            return await _db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<AttendedEvent>> GetCommonEventsAsync(Guid userId, Guid friendId)
        {
            var userEvents = await _db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            var friendEvents = await _db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.UserId == friendId)
                .ToListAsync();

            return userEvents.Where(ue => friendEvents.Any(fe => fe.EventId == ue.EventId)).ToList();
        }

        public async Task<int> GetAttendeeCountAsync(int eventId)
        {
            return await _db.AttendedEvents
                .Include(e => e.Event)
                .Include(e => e.User)
                .Where(e => e.EventId == eventId)
                .CountAsync();
        }
    }
}