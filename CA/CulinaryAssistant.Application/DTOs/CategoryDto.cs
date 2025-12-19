namespace CulinaryAssistant.Application.DTOs;

/// <summary>
/// DTO для категории
/// </summary>
public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? IconName { get; init; }
    public int RecipeCount { get; init; }
}

/// <summary>
/// DTO для создания категории
/// </summary>
public record CategoryCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? IconName { get; init; }
}
