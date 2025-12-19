namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Связь рецепта с категорией (промежуточная таблица для N-N)
/// </summary>
public class RecipeCategory
{
    public int RecipeId { get; private set; }
    public Recipe? Recipe { get; private set; }

    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;

    private RecipeCategory() { }

    public RecipeCategory(int recipeId, int categoryId)
    {
        RecipeId = recipeId;
        CategoryId = categoryId;
    }
}
