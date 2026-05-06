namespace ChatAndEvents.Data.EventsData.Repositories.eventStatisticsRepository;

using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

public class EventStatisticsRepository : IEventStatisticsRepository
{
    private readonly AppDbContext _db;

    public EventStatisticsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ParticipantOverview> GetParticipantOverviewAsync(int eventId)
    {
        var totalParticipants = await _db.AttendedEvents
            .CountAsync(ae => ae.EventId == eventId);

        var discussionUsers = _db.Discussions
            .Where(d => EF.Property<int>(d, "EventId") == eventId)
            .Select(d => EF.Property<Guid>(d, "CreatorId"));

        var memoryUsers = _db.Memories
            .Where(m => m.EventId == eventId)
            .Select(m => m.AuthorId);

        var questUsers = _db.QuestMemories
            .Where(qm => qm.ForQuest.EventId == eventId)
            .Select(qm => qm.Proof.AuthorId);

        var activeParticipants = await discussionUsers
            .Union(memoryUsers)
            .Union(questUsers)
            .CountAsync();

        return new ParticipantOverview
        {
            TotalParticipants = totalParticipants,
            ActiveParticipants = activeParticipants,
            EngagementRate = CalculateRate(activeParticipants, totalParticipants),
        };
    }

    public async Task<EngagementBreakdown> GetEngagementBreakdownAsync(int eventId)
    {
        var totalMessages = await _db.Discussions
            .CountAsync(d => EF.Property<int>(d, "EventId") == eventId);

        var totalMemories = await _db.Memories
            .CountAsync(m => m.EventId == eventId);

        var questSubmissions = _db.QuestMemories
            .Where(qm => qm.ForQuest.EventId == eventId);

        var totalSubmissions = await questSubmissions.CountAsync();
        var approved = await questSubmissions
            .CountAsync(qm => qm.ProofStatus == QuestMemoryStatus.Approved);
        var denied = await questSubmissions
            .CountAsync(qm => qm.ProofStatus == QuestMemoryStatus.Rejected);

        return new EngagementBreakdown
        {
            TotalDiscussionMessages = totalMessages,
            TotalMemories = totalMemories,
            TotalQuestSubmissions = totalSubmissions,
            ApprovedQuests = approved,
            DeniedQuests = denied,
            ApprovedQuestsRate = CalculateRate(approved, totalSubmissions),
            DeniedQuestsRate = CalculateRate(denied, totalSubmissions),
        };
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int eventId)
    {
        var participants = await _db.AttendedEvents
            .Include(ae => ae.User)
            .ThenInclude(u => u.ReputationScore)
            .Where(ae => ae.EventId == eventId)
            .ToListAsync();

        var messageCounts = await _db.Discussions
            .Where(d => EF.Property<int>(d, "EventId") == eventId)
            .GroupBy(d => EF.Property<Guid>(d, "CreatorId"))
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var memoryCounts = await _db.Memories
            .Where(m => m.EventId == eventId)
            .GroupBy(m => m.AuthorId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var questCounts = await _db.QuestMemories
            .Where(qm => qm.ForQuest.EventId == eventId && qm.ProofStatus == QuestMemoryStatus.Approved)
            .GroupBy(qm => qm.Proof.AuthorId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        return participants
            .Select(ae =>
            {
                var messages = messageCounts.GetValueOrDefault(ae.UserId);
                var memories = memoryCounts.GetValueOrDefault(ae.UserId);
                var quests = questCounts.GetValueOrDefault(ae.UserId);

                return new LeaderboardEntry
                {
                    UserName = ae.User.Name,
                    Tier = ae.User.ReputationScore?.Tier ?? StatisticsConstants.DefaultTier,
                    TotalMessages = messages,
                    TotalMemories = memories,
                    QuestsCompleted = quests,
                    TotalScore = messages
                        + (memories * StatisticsConstants.MemoryScoreWeight)
                        + (quests * StatisticsConstants.QuestScoreWeight),
                };
            })
            .OrderByDescending(entry => entry.TotalScore)
            .Take(StatisticsConstants.LeaderboardLimit)
            .ToList();
    }

    public async Task<List<QuestAnalyticsEntry>> GetQuestAnalyticsAsync(int eventId)
    {
        var quests = await _db.Set<Quest>()
            .Where(q => q.EventId == eventId)
            .Select(q => new { q.Id, q.Name })
            .ToListAsync();

        var completedCounts = await _db.QuestMemories
            .Where(qm => qm.ForQuest.EventId == eventId && qm.ProofStatus == QuestMemoryStatus.Approved)
            .GroupBy(qm => qm.QuestId)
            .Select(g => new { QuestId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.QuestId, x => x.Count);

        return quests
            .Select(q => new QuestAnalyticsEntry
            {
                QuestName = q.Name,
                TotalCompletedQuests = completedCounts.GetValueOrDefault(q.Id),
            })
            .OrderByDescending(entry => entry.TotalCompletedQuests)
            .ToList();
    }

    private static double CalculateRate(int value, int total)
    {
        return total == 0 ? 0 : (double)value / total * 100;
    }

    private static class StatisticsConstants
    {
        public const int MemoryScoreWeight = 2;
        public const int QuestScoreWeight = 3;
        public const string DefaultTier = "Newcomer";
        public const int LeaderboardLimit = 100;
    }
}
