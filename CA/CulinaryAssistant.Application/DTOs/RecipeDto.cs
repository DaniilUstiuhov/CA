using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Application.DTOs;

/// <summary>
/// DTO для отображения рецепта
/// </summary>
public record RecipeDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Cuisine { get; init; } = string.Empty;
    public DishType DishType { get; init; }
    public RecipeStatus Status { get; init; }
    public int CookingTimeMinutes { get; init; }
    public int Servings { get; init; }
    public string? Instructions { get; init; }
    public string? ImagePath { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ArchivedAt { get; init; }
    public List<RecipeIngredientDto> Ingredients { get; init; } = new();
    public List<CategoryDto> Categories { get; init; } = new();
}

/// <summary>
/// DTO для создания/редактирования рецепта
/// </summary>
public record RecipeCreateDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Cuisine { get; init; } = string.Empty;
    public DishType DishType { get; init; }
    public int CookingTimeMinutes { get; init; }
    public int Servings { get; init; }
    public string? Instructions { get; init; }
    public string? ImagePath { get; init; }
}

/// <summary>
/// DTO для обновления рецепта
/// </summary>
public record RecipeUpdateDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Cuisine { get; init; } = string.Empty;
    public DishType DishType { get; init; }
    public int CookingTimeMinutes { get; init; }
    public int Servings { get; init; }
    public string? Instructions { get; init; }
    public string? ImagePath { get; init; }
}

/// <summary>
/// DTO для краткого отображения рецепта в списке
/// </summary>
public record RecipeListItemDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Cuisine { get; init; } = string.Empty;
    public DishType DishType { get; init; }
    public RecipeStatus Status { get; init; }
    public int CookingTimeMinutes { get; init; }
    public int IngredientsCount { get; init; }
}

/// <summary>
/// DTO для фильтрации рецептов
/// </summary>
public record RecipeFilterDto
{
    public string? SearchTerm { get; init; }
    public RecipeStatus? Status { get; init; }
    public DishType? DishType { get; init; }
    public string? Cuisine { get; init; }
    public int? CategoryId { get; init; }
}
