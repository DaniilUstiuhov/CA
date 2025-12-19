using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Продукт в инвентаре (производный от Item)
/// </summary>
public class InventoryItem : Item
{
    public DateTime ExpirationDate { get; private set; }
    public string? StorageLocation { get; private set; }
    public bool IsExpired => DateTime.UtcNow.Date > ExpirationDate.Date;
    public bool IsExpiringSoon => !IsExpired && ExpirationDate.Date <= DateTime.UtcNow.Date.AddDays(3);
    public int DaysUntilExpiration => (ExpirationDate.Date - DateTime.UtcNow.Date).Days;

    private InventoryItem() : base() { }

    public InventoryItem(string name, double quantity, MeasurementUnit unit, DateTime expirationDate, string? storageLocation = null)
        : base(name, quantity, unit)
    {
        SetExpirationDate(expirationDate);
        StorageLocation = storageLocation;
    }

    public void SetExpirationDate(DateTime expirationDate)
    {
        ExpirationDate = expirationDate;
        SetUpdated();
    }

    public void SetStorageLocation(string? location)
    {
        StorageLocation = location?.Trim();
        SetUpdated();
    }

    public void Use(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Количество должно быть положительным", nameof(amount));

        if (amount > Quantity)
            throw new InvalidOperationException($"Недостаточно продукта. Доступно: {Quantity}, запрошено: {amount}");

        SetQuantity(Quantity - amount);
    }

    public void Replenish(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Количество должно быть положительным", nameof(amount));

        SetQuantity(Quantity + amount);
    }
}
