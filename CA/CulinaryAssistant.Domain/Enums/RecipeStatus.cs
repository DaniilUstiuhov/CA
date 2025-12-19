namespace CulinaryAssistant.Domain.Enums;

/// <summary>
/// Статус рецепта - реализует workflow: Draft → Published → Archived
/// </summary>
public enum RecipeStatus
{
    /// <summary>
    /// Черновик - рецепт создан, но еще не опубликован
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Опубликован - рецепт доступен для использования
    /// </summary>
    Published = 1,

    /// <summary>
    /// Архивирован - рецепт больше не используется
    /// </summary>
    Archived = 2
}
