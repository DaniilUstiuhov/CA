using CulinaryAssistant.Domain.Entities;

namespace CulinaryAssistant.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория списков покупок
/// </summary>
public interface IShoppingListRepository : IRepository<ShoppingList>
{
    Task<ShoppingList?> GetWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingList>> GetActiveListsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingList>> GetCompletedListsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingList>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}
