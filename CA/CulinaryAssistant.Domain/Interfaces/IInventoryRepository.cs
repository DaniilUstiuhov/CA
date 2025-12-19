using CulinaryAssistant.Domain.Entities;

namespace CulinaryAssistant.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория инвентаря
/// </summary>
public interface IInventoryRepository : IRepository<InventoryItem>
{
    Task<IReadOnlyList<InventoryItem>> GetExpiredItemsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetExpiringSoonItemsAsync(int daysThreshold = 3, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetByStorageLocationAsync(string location, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
