using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Список покупок (Aggregate Root)
/// </summary>
public class ShoppingList : Entity
{
    private readonly List<ShoppingItem> _items = new();

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyCollection<ShoppingItem> Items => _items.AsReadOnly();

    public int TotalItems => _items.Count;
    public int PurchasedItems => _items.Count(i => i.IsPurchased);
    public decimal TotalEstimatedPrice => _items.Where(i => i.EstimatedPrice.HasValue).Sum(i => i.EstimatedPrice!.Value);
    public double CompletionPercentage => TotalItems > 0 ? (double)PurchasedItems / TotalItems * 100 : 0;

    private ShoppingList() { }

    public ShoppingList(string name, string? description = null)
    {
        SetName(name);
        Description = description?.Trim();
        IsCompleted = false;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название списка не может быть пустым", nameof(name));

        Name = name.Trim();
        SetUpdated();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        SetUpdated();
    }

    public ShoppingItem AddItem(string name, double quantity, MeasurementUnit unit, 
        decimal? estimatedPrice = null, string? preferredStore = null, string? notes = null)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Нельзя добавлять товары в завершённый список");

        var existingItem = _items.FirstOrDefault(i => 
            i.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !i.IsPurchased);

        if (existingItem != null)
        {
            existingItem.SetQuantity(existingItem.Quantity + quantity);
            return existingItem;
        }

        var item = new ShoppingItem(name, quantity, unit, Id, estimatedPrice, preferredStore, notes);
        _items.Add(item);
        SetUpdated();
        return item;
    }

    public void RemoveItem(ShoppingItem item)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Нельзя удалять товары из завершённого списка");

        _items.Remove(item);
        SetUpdated();
    }

    public void MarkAsCompleted()
    {
        if (IsCompleted)
            return;

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void Reopen()
    {
        IsCompleted = false;
        CompletedAt = null;
        SetUpdated();
    }

    public void ClearPurchasedItems()
    {
        _items.RemoveAll(i => i.IsPurchased);
        SetUpdated();
    }
}
