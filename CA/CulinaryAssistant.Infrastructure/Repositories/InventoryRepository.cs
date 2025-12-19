using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Interfaces;
using CulinaryAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CulinaryAssistant.Infrastructure.Repositories;

/// <summary>
/// Inventory repository with expiration tracking queries
/// </summary>
public class InventoryRepository : Repository<InventoryItem>, IInventoryRepository
{
    public InventoryRepository(CulinaryDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InventoryItem>> GetExpiredItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ExpirationDate.Date < DateTime.Today)
            .OrderBy(i => i.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetExpiringSoonItemsAsync(int daysThreshold = 3, CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTime.Today.AddDays(daysThreshold);
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.ExpirationDate.Date >= DateTime.Today &&
                        i.ExpirationDate.Date <= thresholdDate)
            .OrderBy(i => i.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItem>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.Name.ToLower().Contains(searchTerm.ToLower()))
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetByStorageLocationAsync(string location, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => i.StorageLocation != null && i.StorageLocation.ToLower().Contains(location.ToLower()))
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<InventoryItem?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<InventoryItem>> SearchAsync(
        string? searchText = null,
        string? storageLocation = null,
        bool? expiredOnly = null,
        bool? expiringSoon = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(searchText));
        }

        if (!string.IsNullOrWhiteSpace(storageLocation))
        {
            query = query.Where(i => i.StorageLocation != null && 
                                     i.StorageLocation.ToLower().Contains(storageLocation.ToLower()));
        }

        if (expiredOnly == true)
        {
            query = query.Where(i => i.ExpirationDate.Date < DateTime.Today);
        }
        else if (expiringSoon == true)
        {
            var threshold = DateTime.Today.AddDays(3);
            query = query.Where(i => i.ExpirationDate.Date >= DateTime.Today &&
                                     i.ExpirationDate.Date <= threshold);
        }

        return await query
            .OrderBy(i => i.ExpirationDate)
            .ThenBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAllStorageLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(i => !string.IsNullOrEmpty(i.StorageLocation))
            .Select(i => i.StorageLocation!)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync(cancellationToken);
    }

    public override async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(i => i.ExpirationDate)
            .ThenBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }
}
