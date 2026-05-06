namespace ChatAndEvents.Data.EventsData.Repositories.reputationRepository;

using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;


public class ReputationRepository : IReputationRepository
{
    private readonly AppDbContext _db;
    
    public ReputationRepository(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task SetReputationAsync(Guid userId, int reputationPoints, string tier)
    {
        await this.SetReputationAsync(new UserReputationScore
        {
            UserId = userId,
            ReputationPoints = reputationPoints,
            Tier = tier,
        });
    }
    
    public async Task SetReputationAsync(UserReputationScore reputationScore)
    {
        if (reputationScore == null)
            throw new ArgumentException("Reputation score is required.", nameof(reputationScore));
        
        var existingScore = await _db.UserReputationScores
            .FirstOrDefaultAsync(u => u.UserId == reputationScore.UserId);

        if (existingScore == null)
        {
            _db.UserReputationScores.Add(reputationScore);
        }
        else
        {
            existingScore.ReputationPoints = reputationScore.ReputationPoints;
            existingScore.Tier = reputationScore.Tier;
            _db.UserReputationScores.Update(existingScore);
        }

        await _db.SaveChangesAsync();
    }
    
    public async Task<UserReputationScore> GetReputationScoreAsync(Guid userId)
    {
        var reputationScore = await _db.UserReputationScores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        if (reputationScore == null)
        {
            return new UserReputationScore
            {
                UserId = userId,
                ReputationPoints = 0,
                Tier = SharedReputationConstants.NewcomerTier,
            };
        }

        return reputationScore;
    }
    
    public async Task<int> GetReputationPointsAsync(Guid userId)
    {
        var reputationScore = await this.GetReputationScoreAsync(userId);
        return reputationScore.ReputationPoints;
    }
    
    public async Task<string> GetTierAsync(Guid userId)
    {
        var reputationScore = await this.GetReputationScoreAsync(userId);
        return reputationScore.Tier;
    }
}

public static class SharedReputationConstants
{
    public const string NewcomerTier = "Newcomer";
}
