namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Категория рецептов (для связи N-N)
/// </summary>
public class Category : Entity
{
    private readonly List<RecipeCategory> _recipeCategories = new();

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconName { get; private set; }

    public IReadOnlyCollection<RecipeCategory> RecipeCategories => _recipeCategories.AsReadOnly();

    private Category() { }

    public Category(string name, string? description = null, string? iconName = null)
    {
        SetName(name);
        Description = description?.Trim();
        IconName = iconName;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название категории не может быть пустым", nameof(name));

        Name = name.Trim();
        SetUpdated();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        SetUpdated();
    }

    public void SetIconName(string? iconName)
    {
        IconName = iconName;
        SetUpdated();
    }
}
