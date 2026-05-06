using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories;

public class QuestMemoryRepository : IQuestMemoryRepository
{
    private readonly AppDbContext _db;

    public QuestMemoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> AddMemoryAsync(Memory proofMemory)
    {
        _db.Memories.Add(proofMemory);
        await _db.SaveChangesAsync();
        return proofMemory.MemoryId;
    }

    public async Task SubmitProofAsync(Quest quest, Memory proof)
    {
        var questMemory = new QuestMemory
        {
            ForQuest = quest,
            Proof = proof,
            QuestId = quest.Id,
            MemoryId = proof.MemoryId,
            ProofStatus = QuestMemoryStatus.Submitted
        };
        _db.Set<QuestMemory>().Add(questMemory);
        await _db.SaveChangesAsync();
    }

    public async Task<List<QuestMemory>> GetRawSubmissionsForUser(User user)
    {
        return await _db.Set<QuestMemory>()
            .Include(qm => qm.ForQuest)
            .Include(qm => qm.Proof)
                .ThenInclude(m => m.Author)
            .Where(qm => qm.Proof.AuthorId == user.UserId)
            .ToListAsync();
    }

    public async Task<List<QuestMemory>> GetProofsForQuestAsync(Quest quest)
    {
        return await _db.Set<QuestMemory>()
            .Include(qm => qm.Proof)
                .ThenInclude(m => m.Author)
            .Include(qm => qm.ForQuest)
            .Where(qm => qm.QuestId == quest.Id && qm.ProofStatus == QuestMemoryStatus.Submitted)
            .ToListAsync();
    }

    public async Task ChangeProofStatusAsync(QuestMemory proof)
    {
        var questMemory = await _db.Set<QuestMemory>()
            .FirstOrDefaultAsync(qm => qm.QuestId == proof.QuestId && qm.MemoryId == proof.MemoryId);

        if (questMemory != null)
        {
            questMemory.ProofStatus = proof.ProofStatus;
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteProofAsync(QuestMemory proof)
    {
        var questMemory = await _db.Set<QuestMemory>()
            .FirstOrDefaultAsync(qm => qm.QuestId == proof.QuestId && qm.MemoryId == proof.MemoryId);

        if (questMemory != null)
        {
            _db.Set<QuestMemory>().Remove(questMemory);
            await _db.SaveChangesAsync();
        }
    }
}