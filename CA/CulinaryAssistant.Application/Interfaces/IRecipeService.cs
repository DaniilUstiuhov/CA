using CulinaryAssistant.Application.DTOs;

namespace CulinaryAssistant.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса рецептов (Application Layer)
/// </summary>
public interface IRecipeService
{
    Task<RecipeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RecipeDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecipeListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecipeListItemDto>> SearchAsync(RecipeFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAllCuisinesAsync(CancellationToken cancellationToken = default);
    Task<RecipeDto> CreateAsync(RecipeCreateDto dto, CancellationToken cancellationToken = default);
    Task<RecipeDto> UpdateAsync(RecipeUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    // Workflow operations
    Task<RecipeDto> PublishAsync(int id, CancellationToken cancellationToken = default);
    Task<RecipeDto> ArchiveAsync(int id, CancellationToken cancellationToken = default);
    Task<RecipeDto> RestoreAsync(int id, CancellationToken cancellationToken = default);
    Task<RecipeDto> ReturnToDraftAsync(int id, CancellationToken cancellationToken = default);
    
    // Ingredient operations
    Task<RecipeDto> AddIngredientAsync(int recipeId, RecipeIngredientCreateDto dto, CancellationToken cancellationToken = default);
    Task<RecipeDto> RemoveIngredientAsync(int recipeId, int ingredientId, CancellationToken cancellationToken = default);
    
    // Category operations
    Task<RecipeDto> AddCategoryAsync(int recipeId, int categoryId, CancellationToken cancellationToken = default);
    Task<RecipeDto> RemoveCategoryAsync(int recipeId, int categoryId, CancellationToken cancellationToken = default);
}
