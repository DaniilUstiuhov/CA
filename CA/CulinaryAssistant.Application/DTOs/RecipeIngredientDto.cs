using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Application.DTOs;

/// <summary>
/// DTO для ингредиента рецепта
/// </summary>
public record RecipeIngredientDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Amount { get; init; }
    public MeasurementUnit Unit { get; init; }
    public bool IsOptional { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO для создания ингредиента
/// </summary>
public record RecipeIngredientCreateDto
{
    public string Name { get; init; } = string.Empty;
    public double Amount { get; init; }
    public MeasurementUnit Unit { get; init; }
    public bool IsOptional { get; init; }
    public string? Notes { get; init; }
}
