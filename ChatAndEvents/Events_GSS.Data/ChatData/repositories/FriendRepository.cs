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
    public class FriendRepository : IFriendRepository
    {
        private readonly AppDbContext _db;

        public FriendRepository(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Friend?> GetFriendshipAsync(Guid firstUserId, Guid secondUserId)
        {
            return await _db.Friends.FirstOrDefaultAsync(friendship =>
                (friendship.UserId1 == firstUserId && friendship.UserId2 == secondUserId) ||
                (friendship.UserId1 == secondUserId && friendship.UserId2 == firstUserId));
        }

        public async Task<List<Friend>> GetAllFriendshipsForUserAsync(Guid targetUserId)
        {
            return await _db.Friends
                .Where(friendship => friendship.UserId1 == targetUserId || friendship.UserId2 == targetUserId)
                .ToListAsync();
        }

        public async Task<List<Friend>> GetPendingRequestsForUserAsync(Guid targetUserId)
        {
            return await _db.Friends
                .Where(friendship => friendship.UserId2 == targetUserId && friendship.Status == FriendStatus.Pending)
                .ToListAsync();
        }

        public async Task<List<Friend>> GetAcceptedFriendsAsync(Guid targetUserId)
        {
            return await _db.Friends
                .Where(friendship =>
                    (friendship.UserId1 == targetUserId || friendship.UserId2 == targetUserId) &&
                    friendship.Status == FriendStatus.Accepted)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetMutualFriendIdentifiersAsync(Guid firstUserId, Guid secondUserId)
        {
            var firstUserFriends = await _db.Friends
                .Where(friendship =>
                    friendship.Status == FriendStatus.Accepted &&
                    (friendship.UserId1 == firstUserId || friendship.UserId2 == firstUserId))
                .Select(friendship => friendship.UserId1 == firstUserId ? friendship.UserId2 : friendship.UserId1)
                .ToListAsync();

            var secondUserFriends = await _db.Friends
                .Where(friendship =>
                    friendship.Status == FriendStatus.Accepted &&
                    (friendship.UserId1 == secondUserId || friendship.UserId2 == secondUserId))
                .Select(friendship => friendship.UserId1 == secondUserId ? friendship.UserId2 : friendship.UserId1)
                .ToListAsync();

            return firstUserFriends
                .Intersect(secondUserFriends)
                .ToList();
        }

        public async Task<bool> CheckIfFriendsAsync(Guid firstUserId, Guid secondUserId)
        {
            return await _db.Friends.AnyAsync(friendship =>
                friendship.Status == FriendStatus.Accepted &&
                ((friendship.UserId1 == firstUserId && friendship.UserId2 == secondUserId) ||
                 (friendship.UserId1 == secondUserId && friendship.UserId2 == firstUserId)));
        }

        public async Task CreateFriendshipAsync(Friend newFriendship)
        {
            _db.Friends.Add(newFriendship);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateFriendshipStatusAsync(Guid firstUserId, Guid secondUserId, FriendStatus newStatus)
        {
            var friendship = await GetFriendshipAsync(firstUserId, secondUserId);
            if (friendship == null)
            {
                return;
            }

            friendship.Status = newStatus;
            await _db.SaveChangesAsync();
        }

        public async Task SetMatchStatusAsync(Guid firstUserId, Guid secondUserId, bool isMatchStatus)
        {
            var friendship = await GetFriendshipAsync(firstUserId, secondUserId);
            if (friendship == null)
            {
                return;
            }

            friendship.IsMatch = isMatchStatus;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteFriendshipAsync(Guid firstUserId, Guid secondUserId)
        {
            var friendship = await GetFriendshipAsync(firstUserId, secondUserId);
            if (friendship == null)
            {
                return;
            }

            _db.Friends.Remove(friendship);
            await _db.SaveChangesAsync();
        }
    }
}