namespace CulinaryAssistant.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса экспорта (Application Layer)
/// </summary>
public interface IExportService
{
    Task<byte[]> ExportRecipesToCsvAsync(CancellationToken cancellationToken = default);
    Task<byte[]> ExportRecipeDetailToCsvAsync(int recipeId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportInventoryToCsvAsync(CancellationToken cancellationToken = default);
    Task<byte[]> ExportShoppingListToCsvAsync(int shoppingListId, CancellationToken cancellationToken = default);
}
