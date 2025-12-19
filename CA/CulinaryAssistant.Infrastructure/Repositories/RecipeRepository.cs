using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Enums;
using CulinaryAssistant.Domain.Interfaces;
using CulinaryAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CulinaryAssistant.Infrastructure.Repositories;

/// <summary>
/// Recipe repository with specialized queries
/// </summary>
public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(CulinaryDbContext context) : base(context) { }

    public override async Task<Recipe?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Ingredients)
            .Include(r => r.RecipeCategories)
                .ThenInclude(rc => rc.Category)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Recipe?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Ingredients)
            .Include(r => r.RecipeCategories)
                .ThenInclude(rc => rc.Category)
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Recipe>> GetByStatusAsync(RecipeStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Include(r => r.RecipeCategories)
                .ThenInclude(rc => rc.Category)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Recipe>> SearchAsync(
        string? searchText = null,
        DishType? dishType = null,
        RecipeStatus? status = null,
        string? cuisine = null,
        int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Include(r => r.RecipeCategories)
                .ThenInclude(rc => rc.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(searchText) ||
                r.Description.ToLower().Contains(searchText) ||
                r.Code.ToLower().Contains(searchText));
        }

        if (dishType.HasValue)
            query = query.Where(r => r.DishType == dishType.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(cuisine))
            query = query.Where(r => r.Cuisine.ToLower().Contains(cuisine.ToLower()));

        if (categoryId.HasValue)
            query = query.Where(r => r.RecipeCategories.Any(rc => rc.CategoryId == categoryId.Value));

        return await query
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r => r.Code == code);
        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);
        
        return await query.AnyAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Include(r => r.RecipeCategories)
                .ThenInclude(rc => rc.Category)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAllCuisinesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(r => !string.IsNullOrEmpty(r.Cuisine))
            .Select(r => r.Cuisine)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }
}
