using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Базовый класс для всех типов продуктов (TPH наследование)
/// Discriminator: ItemType
/// </summary>
public abstract class Item : Entity
{
    public string Name { get; protected set; } = string.Empty;
    public double Quantity { get; protected set; }
    public MeasurementUnit Unit { get; protected set; }

    protected Item() { }

    protected Item(string name, double quantity, MeasurementUnit unit)
    {
        SetName(name);
        SetQuantity(quantity);
        Unit = unit;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название не может быть пустым", nameof(name));

        Name = name.Trim();
        SetUpdated();
    }

    public void SetQuantity(double quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Количество не может быть отрицательным", nameof(quantity));

        Quantity = quantity;
        SetUpdated();
    }

    public void SetUnit(MeasurementUnit unit)
    {
        Unit = unit;
        SetUpdated();
    }
}
