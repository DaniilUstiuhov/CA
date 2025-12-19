using CulinaryAssistant.Application.DTOs;

namespace CulinaryAssistant.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса списков покупок (Application Layer)
/// </summary>
public interface IShoppingListService
{
    Task<ShoppingListDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingListSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingListSummaryDto>> GetActiveListsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShoppingListSummaryDto>> GetCompletedListsAsync(CancellationToken cancellationToken = default);
    Task<ShoppingListDto> CreateAsync(ShoppingListCreateDto dto, CancellationToken cancellationToken = default);
    Task<ShoppingListDto> UpdateAsync(int id, ShoppingListCreateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    // Item operations
    Task<ShoppingListDto> AddItemAsync(int listId, ShoppingItemCreateDto dto, CancellationToken cancellationToken = default);
    Task<ShoppingListDto> RemoveItemAsync(int listId, int itemId, CancellationToken cancellationToken = default);
    Task<ShoppingListDto> MarkItemPurchasedAsync(int listId, int itemId, CancellationToken cancellationToken = default);
    Task<ShoppingListDto> MarkItemNotPurchasedAsync(int listId, int itemId, CancellationToken cancellationToken = default);
    
    // List operations
    Task<ShoppingListDto> MarkAsCompletedAsync(int id, CancellationToken cancellationToken = default);
    Task<ShoppingListDto> ReopenAsync(int id, CancellationToken cancellationToken = default);
    Task<ShoppingListDto> ClearPurchasedItemsAsync(int id, CancellationToken cancellationToken = default);
}
