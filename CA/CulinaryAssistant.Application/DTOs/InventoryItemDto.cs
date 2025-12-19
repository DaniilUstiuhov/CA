using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Application.DTOs;

/// <summary>
/// DTO для элемента инвентаря
/// </summary>
public record InventoryItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Quantity { get; init; }
    public MeasurementUnit Unit { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? StorageLocation { get; init; }
    public bool IsExpired { get; init; }
    public bool IsExpiringSoon { get; init; }
    public int DaysUntilExpiration { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO для создания элемента инвентаря
/// </summary>
public record InventoryItemCreateDto
{
    public string Name { get; init; } = string.Empty;
    public double Quantity { get; init; }
    public MeasurementUnit Unit { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? StorageLocation { get; init; }
}

/// <summary>
/// DTO для обновления элемента инвентаря
/// </summary>
public record InventoryItemUpdateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Quantity { get; init; }
    public MeasurementUnit Unit { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? StorageLocation { get; init; }
}

/// <summary>
/// DTO для фильтрации инвентаря
/// </summary>
public record InventoryFilterDto
{
    public string? SearchTerm { get; init; }
    public string? StorageLocation { get; init; }
    public bool? ShowExpired { get; init; }
    public bool? ShowExpiringSoon { get; init; }
}
