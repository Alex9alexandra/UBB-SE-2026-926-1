using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<List<User>> SearchByUsernameAsync(string query)
        {
            return await _db.Users
                .Where(u => u.Username.Contains(query))
                .ToListAsync();
        }

        public async Task CreateAsync(User user)
        {
            _db.Users.Add(user);

            var eventsUser = new ChatAndEvents.Data.EventsData.Models.User
            {
                UserId = user.Id,
                Name = user.Username,
                ReputationPoints = 0
            };
            _db.Set<ChatAndEvents.Data.EventsData.Models.User>().Add(eventsUser);

            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
          
        public async Task UpdatePasswordAsync(Guid id, string passwordHash)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return;
            user.PasswordHash = passwordHash;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
    }
}