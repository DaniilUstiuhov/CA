using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Enums;
using CulinaryAssistant.Domain.Exceptions;
using CulinaryAssistant.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CulinaryAssistant.Application.Services;

/// <summary>
/// Сервис рецептов (Application Layer)
/// </summary>
public class RecipeService : IRecipeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecipeService> _logger;
    private readonly IValidator<RecipeCreateDto> _createValidator;
    private readonly IValidator<RecipeUpdateDto> _updateValidator;
    private readonly IValidator<RecipeIngredientCreateDto> _ingredientValidator;

    public RecipeService(
        IUnitOfWork unitOfWork,
        ILogger<RecipeService> logger,
        IValidator<RecipeCreateDto> createValidator,
        IValidator<RecipeUpdateDto> updateValidator,
        IValidator<RecipeIngredientCreateDto> ingredientValidator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _ingredientValidator = ingredientValidator;
    }

    public async Task<RecipeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recipe by ID: {Id}", id);
        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(id, cancellationToken);
        return recipe != null ? MapToDto(recipe) : null;
    }

    public async Task<RecipeDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recipe by code: {Code}", code);
        var recipe = await _unitOfWork.Recipes.GetByCodeAsync(code, cancellationToken);
        if (recipe == null) return null;
        
        var fullRecipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(recipe.Id, cancellationToken);
        return fullRecipe != null ? MapToDto(fullRecipe) : null;
    }

    public async Task<IReadOnlyList<RecipeListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all recipes");
        var recipes = await _unitOfWork.Recipes.GetAllAsync(cancellationToken);
        return recipes.Select(MapToListItemDto).ToList();
    }

    public async Task<IReadOnlyList<RecipeListItemDto>> SearchAsync(RecipeFilterDto filter, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching recipes with filter: {@Filter}", filter);
        var recipes = await _unitOfWork.Recipes.SearchAsync(
            filter.SearchTerm, filter.Status, filter.DishType, filter.Cuisine, cancellationToken);
        return recipes.Select(MapToListItemDto).ToList();
    }

    public async Task<IReadOnlyList<string>> GetAllCuisinesAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Recipes.GetAllCuisinesAsync(cancellationToken);
    }

    public async Task<RecipeDto> CreateAsync(RecipeCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new recipe: {Name}", dto.Name);

        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        if (await _unitOfWork.Recipes.CodeExistsAsync(dto.Code, cancellationToken: cancellationToken))
            throw new DomainException($"Рецепт с кодом '{dto.Code}' уже существует");

        var recipe = new Recipe(
            dto.Code, dto.Name, dto.Cuisine, dto.DishType,
            dto.CookingTimeMinutes, dto.Servings, dto.Description, dto.Instructions);

        if (!string.IsNullOrWhiteSpace(dto.ImagePath))
            recipe.SetImagePath(dto.ImagePath);

        await _unitOfWork.Recipes.AddAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipe created with ID: {Id}", recipe.Id);
        return MapToDto(recipe);
    }

    public async Task<RecipeDto> UpdateAsync(RecipeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating recipe: {Id}", dto.Id);

        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(dto.Id, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {dto.Id} не найден");

        if (!recipe.CanEdit)
            throw new DomainException("Нельзя редактировать опубликованный или архивированный рецепт. Сначала верните его в черновик.");

        if (await _unitOfWork.Recipes.CodeExistsAsync(dto.Code, dto.Id, cancellationToken))
            throw new DomainException($"Рецепт с кодом '{dto.Code}' уже существует");

        recipe.SetCode(dto.Code);
        recipe.SetName(dto.Name);
        recipe.SetCuisine(dto.Cuisine);
        recipe.SetDishType(dto.DishType);
        recipe.SetCookingTime(dto.CookingTimeMinutes);
        recipe.SetServings(dto.Servings);
        recipe.SetDescription(dto.Description);
        recipe.SetInstructions(dto.Instructions);
        recipe.SetImagePath(dto.ImagePath);

        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipe updated: {Id}", recipe.Id);
        return MapToDto(recipe);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting recipe: {Id}", id);

        var recipe = await _unitOfWork.Recipes.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {id} не найден");

        await _unitOfWork.Recipes.DeleteAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipe deleted: {Id}", id);
    }

    #region Workflow Operations

    public async Task<RecipeDto> PublishAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Publishing recipe: {Id}", id);

        var recipe = await _unitOfWork.Recipes.GetWithIngredientsAsync(id, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {id} не найден");

        recipe.Publish();
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipe published: {Id}", id);
        return MapToDto(recipe);
    }

    public async Task<RecipeDto> ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Archiving recipe: {Id}", id);

        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(id, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {id} не найден");

        recipe.Archive();
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipe archived: {Id}", id);
        return MapToDto(recipe);
    }

    public async Task<RecipeDto> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Restoring recipe: {Id}", id);

        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(id, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {id} не найден");

        recipe.Restore();
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipe restored: {Id}", id);
        return MapToDto(recipe);
    }

    public async Task<RecipeDto> ReturnToDraftAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Returning recipe to draft: {Id}", id);

        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(id, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {id} не найден");

        recipe.ReturnToDraft();
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recipe returned to draft: {Id}", id);
        return MapToDto(recipe);
    }

    #endregion

    #region Ingredient Operations

    public async Task<RecipeDto> AddIngredientAsync(int recipeId, RecipeIngredientCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding ingredient to recipe: {RecipeId}", recipeId);

        var validationResult = await _ingredientValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var recipe = await _unitOfWork.Recipes.GetWithIngredientsAsync(recipeId, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {recipeId} не найден");

        recipe.AddIngredient(dto.Name, dto.Amount, dto.Unit, dto.IsOptional, dto.Notes);
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recipe);
    }

    public async Task<RecipeDto> RemoveIngredientAsync(int recipeId, int ingredientId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing ingredient {IngredientId} from recipe: {RecipeId}", ingredientId, recipeId);

        var recipe = await _unitOfWork.Recipes.GetWithIngredientsAsync(recipeId, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {recipeId} не найден");

        var ingredient = recipe.Ingredients.FirstOrDefault(i => i.Id == ingredientId)
            ?? throw new DomainException($"Ингредиент с ID {ingredientId} не найден");

        recipe.RemoveIngredient(ingredient);
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recipe);
    }

    #endregion

    #region Category Operations

    public async Task<RecipeDto> AddCategoryAsync(int recipeId, int categoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding category {CategoryId} to recipe: {RecipeId}", categoryId, recipeId);

        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(recipeId, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {recipeId} не найден");

        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new DomainException($"Категория с ID {categoryId} не найдена");

        recipe.AddCategory(category);
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recipe);
    }

    public async Task<RecipeDto> RemoveCategoryAsync(int recipeId, int categoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing category {CategoryId} from recipe: {RecipeId}", categoryId, recipeId);

        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(recipeId, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {recipeId} не найден");

        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new DomainException($"Категория с ID {categoryId} не найдена");

        recipe.RemoveCategory(category);
        await _unitOfWork.Recipes.UpdateAsync(recipe, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(recipe);
    }

    #endregion

    #region Mapping

    private static RecipeDto MapToDto(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Code = recipe.Code,
        Name = recipe.Name,
        Description = recipe.Description,
        Cuisine = recipe.Cuisine,
        DishType = recipe.DishType,
        Status = recipe.Status,
        CookingTimeMinutes = recipe.CookingTimeMinutes,
        Servings = recipe.Servings,
        Instructions = recipe.Instructions,
        ImagePath = recipe.ImagePath,
        CreatedAt = recipe.CreatedAt,
        PublishedAt = recipe.PublishedAt,
        ArchivedAt = recipe.ArchivedAt,
        Ingredients = recipe.Ingredients.Select(i => new RecipeIngredientDto
        {
            Id = i.Id,
            Name = i.Name,
            Amount = i.Amount,
            Unit = i.Unit,
            IsOptional = i.IsOptional,
            Notes = i.Notes
        }).ToList(),
        Categories = recipe.RecipeCategories
            .Where(rc => rc.Category != null)
            .Select(rc => new CategoryDto
            {
                Id = rc.Category!.Id,
                Name = rc.Category.Name,
                Description = rc.Category.Description,
                IconName = rc.Category.IconName
            }).ToList()
    };

    private static RecipeListItemDto MapToListItemDto(Recipe recipe) => new()
    {
        Id = recipe.Id,
        Code = recipe.Code,
        Name = recipe.Name,
        Cuisine = recipe.Cuisine,
        DishType = recipe.DishType,
        Status = recipe.Status,
        CookingTimeMinutes = recipe.CookingTimeMinutes,
        IngredientsCount = recipe.Ingredients.Count
    };

    #endregion
}
