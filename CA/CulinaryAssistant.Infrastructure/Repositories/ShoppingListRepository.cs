using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Interfaces;
using CulinaryAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CulinaryAssistant.Infrastructure.Repositories;

/// <summary>
/// Shopping list repository with aggregated queries
/// </summary>
public class ShoppingListRepository : Repository<ShoppingList>, IShoppingListRepository
{
    public ShoppingListRepository(CulinaryDbContext context) : base(context) { }

    public override async Task<ShoppingList?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.Id == id, cancellationToken);
    }

    public async Task<ShoppingList?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ShoppingList>> GetActiveListsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(sl => sl.Items)
            .Where(sl => !sl.IsCompleted)
            .OrderByDescending(sl => sl.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ShoppingList>> GetCompletedListsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(sl => sl.Items)
            .Where(sl => sl.IsCompleted)
            .OrderByDescending(sl => sl.CompletedAt)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<ShoppingList>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(sl => sl.Items)
            .OrderByDescending(sl => sl.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}
