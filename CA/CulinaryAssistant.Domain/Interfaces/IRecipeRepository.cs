using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория рецептов
/// </summary>
public interface IRecipeRepository : IRepository<Recipe>
{
    Task<Recipe?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Recipe?> GetWithIngredientsAsync(int id, CancellationToken cancellationToken = default);
    Task<Recipe?> GetWithAllRelationsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetByStatusAsync(RecipeStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetByCuisineAsync(string cuisine, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetByDishTypeAsync(DishType dishType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recipe>> SearchAsync(string? searchTerm, RecipeStatus? status, DishType? dishType, 
        string? cuisine, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAllCuisinesAsync(CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);
}
