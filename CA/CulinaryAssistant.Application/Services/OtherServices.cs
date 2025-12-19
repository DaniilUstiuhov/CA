using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Enums;
using CulinaryAssistant.Domain.Exceptions;
using CulinaryAssistant.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CulinaryAssistant.Application.Services;

/// <summary>
/// Сервис категорий (Application Layer)
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoryService> _logger;
    private readonly IValidator<CategoryCreateDto> _validator;

    public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger, IValidator<CategoryCreateDto> validator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _validator = validator;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
        return category != null ? MapToDto(category) : null;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetCategoriesWithRecipeCountAsync(cancellationToken);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var existing = await _unitOfWork.Categories.GetByNameAsync(dto.Name, cancellationToken);
        if (existing != null)
            throw new DomainException($"Категория '{dto.Name}' уже существует");

        var category = new Category(dto.Name, dto.Description, dto.IconName);
        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Категория с ID {id} не найдена");

        category.SetName(dto.Name);
        category.SetDescription(dto.Description);
        category.SetIconName(dto.IconName);

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(category);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Категория с ID {id} не найдена");

        await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static CategoryDto MapToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IconName = category.IconName,
        RecipeCount = category.RecipeCategories.Count
    };
}

/// <summary>
/// Сервис Dashboard (Application Layer)
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting dashboard data");

        var recipes = await _unitOfWork.Recipes.GetAllAsync(cancellationToken);
        var inventory = await _unitOfWork.Inventory.GetAllAsync(cancellationToken);
        var shoppingLists = await _unitOfWork.ShoppingLists.GetActiveListsAsync(cancellationToken);
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        var expiringItems = await _unitOfWork.Inventory.GetExpiringSoonItemsAsync(3, cancellationToken);

        return new DashboardDto
        {
            TotalRecipes = recipes.Count,
            PublishedRecipes = recipes.Count(r => r.Status == RecipeStatus.Published),
            DraftRecipes = recipes.Count(r => r.Status == RecipeStatus.Draft),
            ArchivedRecipes = recipes.Count(r => r.Status == RecipeStatus.Archived),
            TotalInventoryItems = inventory.Count,
            ExpiredItems = inventory.Count(i => i.IsExpired),
            ExpiringSoonItems = inventory.Count(i => i.IsExpiringSoon),
            ActiveShoppingLists = shoppingLists.Count,
            TotalCategories = categories.Count,
            RecentRecipes = recipes.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new RecipeListItemDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Cuisine = r.Cuisine,
                DishType = r.DishType,
                Status = r.Status,
                CookingTimeMinutes = r.CookingTimeMinutes,
                IngredientsCount = r.Ingredients.Count
            }).ToList(),
            ExpiringItems = expiringItems.Take(5).Select(i => new InventoryItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                ExpirationDate = i.ExpirationDate,
                StorageLocation = i.StorageLocation,
                IsExpired = i.IsExpired,
                IsExpiringSoon = i.IsExpiringSoon,
                DaysUntilExpiration = i.DaysUntilExpiration,
                CreatedAt = i.CreatedAt
            }).ToList(),
            ActiveLists = shoppingLists.Take(5).Select(l => new ShoppingListSummaryDto
            {
                Id = l.Id,
                Name = l.Name,
                IsCompleted = l.IsCompleted,
                TotalItems = l.TotalItems,
                PurchasedItems = l.PurchasedItems,
                CompletionPercentage = l.CompletionPercentage,
                CreatedAt = l.CreatedAt
            }).ToList()
        };
    }
}

/// <summary>
/// Сервис экспорта CSV (Application Layer)
/// </summary>
public class ExportService : IExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExportService> _logger;

    public ExportService(IUnitOfWork unitOfWork, ILogger<ExportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<byte[]> ExportRecipesToCsvAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting recipes to CSV");

        var recipes = await _unitOfWork.Recipes.GetAllAsync(cancellationToken);
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("Code;Name;Cuisine;DishType;Status;CookingTimeMinutes;Servings;IngredientsCount;CreatedAt");

        foreach (var recipe in recipes)
        {
            sb.AppendLine($"{Escape(recipe.Code)};{Escape(recipe.Name)};{Escape(recipe.Cuisine)};{recipe.DishType};{recipe.Status};{recipe.CookingTimeMinutes};{recipe.Servings};{recipe.Ingredients.Count};{recipe.CreatedAt:yyyy-MM-dd}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportRecipeDetailToCsvAsync(int recipeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting recipe detail to CSV: {RecipeId}", recipeId);

        var recipe = await _unitOfWork.Recipes.GetWithAllRelationsAsync(recipeId, cancellationToken)
            ?? throw new DomainException($"Рецепт с ID {recipeId} не найден");

        var sb = new StringBuilder();
        
        // Recipe info
        sb.AppendLine($"Recipe: {recipe.Name}");
        sb.AppendLine($"Code: {recipe.Code}");
        sb.AppendLine($"Cuisine: {recipe.Cuisine}");
        sb.AppendLine($"Status: {recipe.Status}");
        sb.AppendLine($"Cooking Time: {recipe.CookingTimeMinutes} min");
        sb.AppendLine($"Servings: {recipe.Servings}");
        sb.AppendLine();
        
        // Ingredients
        sb.AppendLine("Ingredients:");
        sb.AppendLine("Name;Amount;Unit;Optional;Notes");
        foreach (var ingredient in recipe.Ingredients)
        {
            sb.AppendLine($"{Escape(ingredient.Name)};{ingredient.Amount};{ingredient.Unit};{(ingredient.IsOptional ? "Yes" : "No")};{Escape(ingredient.Notes ?? "")}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportInventoryToCsvAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting inventory to CSV");

        var items = await _unitOfWork.Inventory.GetAllAsync(cancellationToken);
        var sb = new StringBuilder();
        
        sb.AppendLine("Name;Quantity;Unit;ExpirationDate;StorageLocation;IsExpired;DaysUntilExpiration");

        foreach (var item in items)
        {
            sb.AppendLine($"{Escape(item.Name)};{item.Quantity};{item.Unit};{item.ExpirationDate:yyyy-MM-dd};{Escape(item.StorageLocation ?? "")};{(item.IsExpired ? "Yes" : "No")};{item.DaysUntilExpiration}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportShoppingListToCsvAsync(int shoppingListId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting shopping list to CSV: {ListId}", shoppingListId);

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(shoppingListId, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {shoppingListId} не найден");

        var sb = new StringBuilder();
        
        sb.AppendLine($"Shopping List: {list.Name}");
        sb.AppendLine($"Status: {(list.IsCompleted ? "Completed" : "Active")}");
        sb.AppendLine($"Progress: {list.PurchasedItems}/{list.TotalItems} ({list.CompletionPercentage:F0}%)");
        sb.AppendLine();
        
        sb.AppendLine("Items:");
        sb.AppendLine("Name;Quantity;Unit;IsPurchased;EstimatedPrice;PreferredStore;Notes");

        foreach (var item in list.Items)
        {
            sb.AppendLine($"{Escape(item.Name)};{item.Quantity};{item.Unit};{(item.IsPurchased ? "Yes" : "No")};{item.EstimatedPrice?.ToString("F2") ?? ""};{Escape(item.PreferredStore ?? "")};{Escape(item.Notes ?? "")}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
