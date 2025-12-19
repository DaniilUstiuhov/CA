using CulinaryAssistant.Application.DTOs;

namespace CulinaryAssistant.Application.Interfaces;

/// <summary>
/// Интерфейс сервиса категорий (Application Layer)
/// </summary>
public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int id, CategoryCreateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
