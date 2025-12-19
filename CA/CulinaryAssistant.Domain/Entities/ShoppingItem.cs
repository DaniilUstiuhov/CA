using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Элемент списка покупок (производный от Item)
/// </summary>
public class ShoppingItem : Item
{
    public bool IsPurchased { get; private set; }
    public DateTime? PurchasedAt { get; private set; }
    public decimal? EstimatedPrice { get; private set; }
    public string? PreferredStore { get; private set; }
    public string? Notes { get; private set; }

    // Навигационное свойство
    public int ShoppingListId { get; private set; }
    public ShoppingList? ShoppingList { get; private set; }

    private ShoppingItem() : base() { }

    public ShoppingItem(string name, double quantity, MeasurementUnit unit, int shoppingListId, 
        decimal? estimatedPrice = null, string? preferredStore = null, string? notes = null)
        : base(name, quantity, unit)
    {
        ShoppingListId = shoppingListId;
        EstimatedPrice = estimatedPrice;
        PreferredStore = preferredStore?.Trim();
        Notes = notes?.Trim();
        IsPurchased = false;
    }

    public void MarkAsPurchased()
    {
        if (IsPurchased)
            return;

        IsPurchased = true;
        PurchasedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void MarkAsNotPurchased()
    {
        IsPurchased = false;
        PurchasedAt = null;
        SetUpdated();
    }

    public void SetEstimatedPrice(decimal? price)
    {
        if (price.HasValue && price.Value < 0)
            throw new ArgumentException("Цена не может быть отрицательной", nameof(price));

        EstimatedPrice = price;
        SetUpdated();
    }

    public void SetPreferredStore(string? store)
    {
        PreferredStore = store?.Trim();
        SetUpdated();
    }

    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        SetUpdated();
    }
}
