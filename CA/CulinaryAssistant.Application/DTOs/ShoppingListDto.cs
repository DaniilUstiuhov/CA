using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Application.DTOs;

/// <summary>
/// DTO для списка покупок
/// </summary>
public record ShoppingListDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int TotalItems { get; init; }
    public int PurchasedItems { get; init; }
    public decimal TotalEstimatedPrice { get; init; }
    public double CompletionPercentage { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<ShoppingItemDto> Items { get; init; } = new();
}

/// <summary>
/// DTO для создания списка покупок
/// </summary>
public record ShoppingListCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// DTO для краткого отображения списка покупок
/// </summary>
public record ShoppingListSummaryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public int TotalItems { get; init; }
    public int PurchasedItems { get; init; }
    public double CompletionPercentage { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO для элемента списка покупок
/// </summary>
public record ShoppingItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Quantity { get; init; }
    public MeasurementUnit Unit { get; init; }
    public bool IsPurchased { get; init; }
    public DateTime? PurchasedAt { get; init; }
    public decimal? EstimatedPrice { get; init; }
    public string? PreferredStore { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO для добавления элемента в список покупок
/// </summary>
public record ShoppingItemCreateDto
{
    public string Name { get; init; } = string.Empty;
    public double Quantity { get; init; }
    public MeasurementUnit Unit { get; init; }
    public decimal? EstimatedPrice { get; init; }
    public string? PreferredStore { get; init; }
    public string? Notes { get; init; }
}
