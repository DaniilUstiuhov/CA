using CulinaryAssistant.Domain.Entities;

namespace CulinaryAssistant.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория категорий
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetCategoriesWithRecipeCountAsync(CancellationToken cancellationToken = default);
}
