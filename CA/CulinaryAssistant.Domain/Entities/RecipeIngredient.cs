using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Ингредиент рецепта (связь 1-N: Recipe → RecipeIngredient)
/// </summary>
public class RecipeIngredient : Entity
{
    public string Name { get; private set; } = string.Empty;
    public double Amount { get; private set; }
    public MeasurementUnit Unit { get; private set; }
    public bool IsOptional { get; private set; }
    public string? Notes { get; private set; }

    // Навигационные свойства
    public int RecipeId { get; private set; }
    public Recipe? Recipe { get; private set; }

    private RecipeIngredient() { }

    public RecipeIngredient(string name, double amount, MeasurementUnit unit, int recipeId, bool isOptional = false, string? notes = null)
    {
        SetName(name);
        SetAmount(amount);
        Unit = unit;
        RecipeId = recipeId;
        IsOptional = isOptional;
        Notes = notes?.Trim();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название ингредиента не может быть пустым", nameof(name));

        Name = name.Trim();
        SetUpdated();
    }

    public void SetAmount(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Количество должно быть положительным", nameof(amount));

        Amount = amount;
        SetUpdated();
    }

    public void SetUnit(MeasurementUnit unit)
    {
        Unit = unit;
        SetUpdated();
    }

    public void SetOptional(bool isOptional)
    {
        IsOptional = isOptional;
        SetUpdated();
    }

    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        SetUpdated();
    }
}
