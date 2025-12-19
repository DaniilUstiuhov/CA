using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Interfaces;
using CulinaryAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CulinaryAssistant.Infrastructure.Repositories;

/// <summary>
/// Category repository implementation
/// </summary>
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(CulinaryDbContext context) : base(context) { }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
