using CulinaryAssistant.Domain.Enums;
using CulinaryAssistant.Domain.Exceptions;

namespace CulinaryAssistant.Domain.Entities;

/// <summary>
/// Рецепт (Aggregate Root) с workflow статусами: Draft → Published → Archived
/// </summary>
public class Recipe : Entity
{
    private readonly List<RecipeIngredient> _ingredients = new();
    private readonly List<RecipeCategory> _recipeCategories = new();

    /// <summary>
    /// Уникальный код рецепта (бизнес-идентификатор)
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Cuisine { get; private set; } = string.Empty;
    public DishType DishType { get; private set; }
    public RecipeStatus Status { get; private set; }
    public int CookingTimeMinutes { get; private set; }
    public int Servings { get; private set; }
    public string? Instructions { get; private set; }
    public string? ImagePath { get; private set; }

    // Workflow dates
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    // Navigation properties
    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();
    public IReadOnlyCollection<RecipeCategory> RecipeCategories => _recipeCategories.AsReadOnly();

    private Recipe() { }

    public Recipe(string code, string name, string cuisine, DishType dishType, 
        int cookingTimeMinutes, int servings, string? description = null, string? instructions = null)
    {
        SetCode(code);
        SetName(name);
        SetCuisine(cuisine);
        DishType = dishType;
        SetCookingTime(cookingTimeMinutes);
        SetServings(servings);
        Description = description?.Trim();
        Instructions = instructions?.Trim();
        Status = RecipeStatus.Draft;
    }

    #region Property Setters

    public void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Код рецепта не может быть пустым", nameof(code));

        if (code.Length > 20)
            throw new ArgumentException("Код рецепта не может быть длиннее 20 символов", nameof(code));

        Code = code.ToUpperInvariant().Trim();
        SetUpdated();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название рецепта не может быть пустым", nameof(name));

        Name = name.Trim();
        SetUpdated();
    }

    public void SetCuisine(string cuisine)
    {
        if (string.IsNullOrWhiteSpace(cuisine))
            throw new ArgumentException("Кухня не может быть пустой", nameof(cuisine));

        Cuisine = cuisine.Trim();
        SetUpdated();
    }

    public void SetDishType(DishType dishType)
    {
        DishType = dishType;
        SetUpdated();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
        SetUpdated();
    }

    public void SetInstructions(string? instructions)
    {
        Instructions = instructions?.Trim();
        SetUpdated();
    }

    public void SetCookingTime(int minutes)
    {
        if (minutes <= 0)
            throw new ArgumentException("Время приготовления должно быть положительным", nameof(minutes));

        CookingTimeMinutes = minutes;
        SetUpdated();
    }

    public void SetServings(int servings)
    {
        if (servings <= 0)
            throw new ArgumentException("Количество порций должно быть положительным", nameof(servings));

        Servings = servings;
        SetUpdated();
    }

    public void SetImagePath(string? imagePath)
    {
        ImagePath = imagePath;
        SetUpdated();
    }

    #endregion

    #region Workflow Methods

    /// <summary>
    /// Опубликовать рецепт (Draft → Published)
    /// </summary>
    public void Publish()
    {
        if (Status != RecipeStatus.Draft)
            throw new DomainException($"Нельзя опубликовать рецепт со статусом '{Status}'. Только черновики могут быть опубликованы.");

        if (_ingredients.Count == 0)
            throw new DomainException("Нельзя опубликовать рецепт без ингредиентов.");

        if (string.IsNullOrWhiteSpace(Instructions))
            throw new DomainException("Нельзя опубликовать рецепт без инструкций по приготовлению.");

        Status = RecipeStatus.Published;
        PublishedAt = DateTime.UtcNow;
        SetUpdated();
    }

    /// <summary>
    /// Архивировать рецепт (Published → Archived)
    /// </summary>
    public void Archive()
    {
        if (Status != RecipeStatus.Published)
            throw new DomainException($"Нельзя архивировать рецепт со статусом '{Status}'. Только опубликованные рецепты могут быть архивированы.");

        Status = RecipeStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
        SetUpdated();
    }

    /// <summary>
    /// Восстановить рецепт из архива (Archived → Published)
    /// </summary>
    public void Restore()
    {
        if (Status != RecipeStatus.Archived)
            throw new DomainException($"Нельзя восстановить рецепт со статусом '{Status}'. Только архивированные рецепты могут быть восстановлены.");

        Status = RecipeStatus.Published;
        ArchivedAt = null;
        SetUpdated();
    }

    /// <summary>
    /// Вернуть в черновик (Published/Archived → Draft)
    /// </summary>
    public void ReturnToDraft()
    {
        if (Status == RecipeStatus.Draft)
            throw new DomainException("Рецепт уже является черновиком.");

        Status = RecipeStatus.Draft;
        PublishedAt = null;
        ArchivedAt = null;
        SetUpdated();
    }

    /// <summary>
    /// Проверяет, можно ли редактировать рецепт
    /// </summary>
    public bool CanEdit => Status == RecipeStatus.Draft;

    #endregion

    #region Ingredient Management

    public RecipeIngredient AddIngredient(string name, double amount, MeasurementUnit unit, bool isOptional = false, string? notes = null)
    {
        if (Status != RecipeStatus.Draft)
            throw new DomainException("Нельзя добавлять ингредиенты в опубликованный или архивированный рецепт. Сначала верните его в черновик.");

        var existingIngredient = _ingredients.FirstOrDefault(i => 
            i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (existingIngredient != null)
            throw new DomainException($"Ингредиент '{name}' уже добавлен в рецепт.");

        var ingredient = new RecipeIngredient(name, amount, unit, Id, isOptional, notes);
        _ingredients.Add(ingredient);
        SetUpdated();
        return ingredient;
    }

    public void RemoveIngredient(RecipeIngredient ingredient)
    {
        if (Status != RecipeStatus.Draft)
            throw new DomainException("Нельзя удалять ингредиенты из опубликованного или архивированного рецепта.");

        if (!_ingredients.Remove(ingredient))
            throw new DomainException("Ингредиент не найден в рецепте.");

        SetUpdated();
    }

    public void ClearIngredients()
    {
        if (Status != RecipeStatus.Draft)
            throw new DomainException("Нельзя очищать ингредиенты опубликованного или архивированного рецепта.");

        _ingredients.Clear();
        SetUpdated();
    }

    #endregion

    #region Category Management

    public void AddCategory(Category category)
    {
        if (_recipeCategories.Any(rc => rc.CategoryId == category.Id))
            return;

        _recipeCategories.Add(new RecipeCategory(Id, category.Id));
        SetUpdated();
    }

    public void RemoveCategory(Category category)
    {
        var recipeCategory = _recipeCategories.FirstOrDefault(rc => rc.CategoryId == category.Id);
        if (recipeCategory != null)
        {
            _recipeCategories.Remove(recipeCategory);
            SetUpdated();
        }
    }

    #endregion
}
