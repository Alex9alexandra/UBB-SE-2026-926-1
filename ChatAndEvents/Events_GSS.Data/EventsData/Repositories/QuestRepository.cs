using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories;

public class QuestRepository : IQuestRepository
{
    private readonly AppDbContext _db;

    public QuestRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> AddQuestAsync(Event toEvent, Quest quest)
    {
        quest.EventId = toEvent.EventId;
        _db.Set<Quest>().Add(quest);
        await _db.SaveChangesAsync();
        return quest.Id;
    }

    public async Task<List<Quest>> GetQuestsAsync(Event fromEvent)
    {
        return await _db.Set<Quest>()
            .Include(q => q.PrerequisiteQuest)
            .Where(q => q.EventId == fromEvent.EventId)
            .ToListAsync();
    }

    public async Task<Quest> GetQuestByIdAsync(int questId)
    {
        return await _db.Set<Quest>()
            .Include(q => q.PrerequisiteQuest)
            .FirstOrDefaultAsync(q => q.Id == questId);
    }

    public async Task DeleteQuestAsync(Quest quest)
    {
        // Delete QuestMemories first due to NoAction on delete
        await _db.Set<QuestMemory>()
            .Where(qm => qm.QuestId == quest.Id)
            .ExecuteDeleteAsync();

        var questEntity = await _db.Set<Quest>().FindAsync(quest.Id);
        if (questEntity != null)
        {
            _db.Set<Quest>().Remove(questEntity);
            await _db.SaveChangesAsync();
        }
    }
}