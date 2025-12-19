using CulinaryAssistant.Application.DTOs;

namespace CulinaryAssistant.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса инвентаря (Application Layer)
/// </summary>
public interface IInventoryService
{
    Task<InventoryItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemDto>> SearchAsync(InventoryFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemDto>> GetExpiredItemsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemDto>> GetExpiringSoonItemsAsync(int daysThreshold = 3, CancellationToken cancellationToken = default);
    Task<InventoryItemDto> CreateAsync(InventoryItemCreateDto dto, CancellationToken cancellationToken = default);
    Task<InventoryItemDto> UpdateAsync(InventoryItemUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task UseItemAsync(int id, double amount, CancellationToken cancellationToken = default);
    Task ReplenishItemAsync(int id, double amount, CancellationToken cancellationToken = default);
}
