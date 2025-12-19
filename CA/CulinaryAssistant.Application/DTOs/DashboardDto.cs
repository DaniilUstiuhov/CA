namespace CulinaryAssistant.Application.DTOs;

/// <summary>
/// DTO для данных Dashboard
/// </summary>
public record DashboardDto
{
    public int TotalRecipes { get; init; }
    public int PublishedRecipes { get; init; }
    public int DraftRecipes { get; init; }
    public int ArchivedRecipes { get; init; }
    public int TotalInventoryItems { get; init; }
    public int ExpiredItems { get; init; }
    public int ExpiringSoonItems { get; init; }
    public int ActiveShoppingLists { get; init; }
    public int TotalCategories { get; init; }
    public List<RecipeListItemDto> RecentRecipes { get; init; } = new();
    public List<InventoryItemDto> ExpiringItems { get; init; } = new();
    public List<ShoppingListSummaryDto> ActiveLists { get; init; } = new();
}
