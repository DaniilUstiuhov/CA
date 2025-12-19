namespace CulinaryAssistant.Domain.Interfaces;

/// <summary>
/// Интерфейс Unit of Work для координации транзакций
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRecipeRepository Recipes { get; }
    IInventoryRepository Inventory { get; }
    IShoppingListRepository ShoppingLists { get; }
    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
