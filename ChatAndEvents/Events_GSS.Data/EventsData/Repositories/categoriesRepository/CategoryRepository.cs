using ChatAndEvents.Data.Database;
using Events_GSS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Events_GSS.Data.Repositories.categoriesRepository;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CategoryRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<Category>().ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int categoryId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<Category>().FirstOrDefaultAsync(c => c.CategoryId == categoryId);
    }
}