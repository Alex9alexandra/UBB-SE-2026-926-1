using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAndEvents.Data.EventsData.Repositories.categoriesRepository;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _db.Set<Category>().ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        return await _db.Set<Category>().FirstOrDefaultAsync(c => c.CategoryId == categoryId);
    }
}