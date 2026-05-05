// <copyright file="MemoryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ChatAndEvents.Data.Database;
    using ChatAndEvents.Data.EventsData.Models;
    using Microsoft.EntityFrameworkCore;

    public class MemoryRepository : IMemoryRepository
    {
        private readonly AppDbContext _db;

        public MemoryRepository(AppDbContext _db)
        {
            this._db = _db;
        }

        public async Task<List<Memory>> GetByEventAsync(int eventId)
        {
            return await _db.Memories
                .AsNoTracking()
                .Include(memory => memory.Event)
                    .ThenInclude(@event => @event.Admin)
                .Include(memory => memory.Author)
                .Where(memory => memory.EventId == eventId)
                .OrderByDescending(memory => memory.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> AddAsync(Memory memory)
        {
            if (memory.Event == null || memory.Author == null)
            {
                throw new ArgumentException("Event and Author are required.", nameof(memory));
            }

            var memoryEntity = new Memory
            {
                PhotoPath = memory.PhotoPath,
                Text = memory.Text,
                CreatedAt = memory.CreatedAt,
                EventId = memory.Event.EventId,
                AuthorId = memory.Author.UserId,
            };

            _db.Memories.Add(memoryEntity);
            await _db.SaveChangesAsync();
            memory.MemoryId = memoryEntity.MemoryId;
            return memoryEntity.MemoryId;
        }

        public async Task DeleteAsync(int memoryId)
        {
            var memory = await _db.Memories.FindAsync(memoryId);
            if (memory == null)
            {
                return;
            }

            _db.Memories.Remove(memory);
            await _db.SaveChangesAsync();
        }

        public async Task AddLikeAsync(int memoryId, Guid userId)
        {
            if (await _db.Memories.FindAsync(memoryId) == null)
            {
                throw new InvalidOperationException("Memory not found.");
            }

            if (await _db.Users.FindAsync(userId) == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var memoryLike = new MemoryLike
            {
                MemoryId = memoryId,
                UserId = userId,
            };

            _db.MemoryLikes.Add(memoryLike);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveLikeAsync(int memoryId, Guid userId)
        {
            var memoryLike = await _db.MemoryLikes.FindAsync(memoryId, userId);
            if (memoryLike == null)
            {
                return;
            }

            _db.MemoryLikes.Remove(memoryLike);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetLikesAsync(int memoryId)
        {
            return await _db.MemoryLikes
                .AsNoTracking()
                .Where(memoryLike => memoryLike.MemoryId == memoryId)
                .Select(memoryLike => memoryLike.UserId)
                .ToListAsync();
        }

        public async Task<Memory?> GetByIdAsync(int memoryId)
        {
            return await _db.Memories
                .AsNoTracking()
                .Include(memory => memory.Event)
                    .ThenInclude(@event => @event.Admin)
                .Include(memory => memory.Author)
                .FirstOrDefaultAsync(memory => memory.MemoryId == memoryId);
        }
    }
}
