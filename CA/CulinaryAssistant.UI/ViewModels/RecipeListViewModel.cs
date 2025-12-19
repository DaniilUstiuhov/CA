using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Domain.Enums;
using CulinaryAssistant.UI.Services;
using Microsoft.Extensions.Logging;

namespace CulinaryAssistant.UI.ViewModels;

/// <summary>
/// Recipe list ViewModel - master view with filtering
/// </summary>
public partial class RecipeListViewModel : ViewModelBase
{
    private readonly IRecipeService _recipeService;
    private readonly ICategoryService _categoryService;
    private readonly IExportService _exportService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<RecipeListViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<RecipeListItemDto> _recipes = new();

    [ObservableProperty]
    private RecipeListItemDto? _selectedRecipe;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _categories = new();

    [ObservableProperty]
    private ObservableCollection<string> _cuisines = new();

    // Filters
    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private DishType? _selectedDishType;

    [ObservableProperty]
    private RecipeStatus? _selectedStatus;

    [ObservableProperty]
    private CategoryDto? _selectedCategory;

    [ObservableProperty]
    private string? _selectedCuisine;

    // Enum values for ComboBoxes
    public IEnumerable<DishType?> DishTypes => new DishType?[] { null }
        .Concat(Enum.GetValues<DishType>().Cast<DishType?>());

    public IEnumerable<RecipeStatus?> Statuses => new RecipeStatus?[] { null }
        .Concat(Enum.GetValues<RecipeStatus>().Cast<RecipeStatus?>());

    public RecipeListViewModel(
        IRecipeService recipeService,
        ICategoryService categoryService,
        IExportService exportService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<RecipeListViewModel> logger)
    {
        _recipeService = recipeService;
        _categoryService = categoryService;
        _exportService = exportService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;
    }

    public async Task LoadAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SearchText = null;
        SelectedDishType = null;
        SelectedStatus = null;
        SelectedCategory = null;
        SelectedCuisine = null;
        await SearchAsync();
    }

    [RelayCommand]
    private void OpenRecipe(RecipeListItemDto recipe)
    {
        _navigationService.NavigateTo("RecipeDetail", recipe.Id);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            // Load categories
            var categories = await _categoryService.GetAllAsync();
            Categories = new ObservableCollection<CategoryDto>(categories);

            // Load cuisines
            var cuisines = await _recipeService.GetAllCuisinesAsync();
            Cuisines = new ObservableCollection<string>(cuisines);

            // Load recipes
            await SearchAsync();

            _logger.LogInformation("Recipe list data loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recipe list data");
            SetError("Ошибка загрузки данных: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsLoading = true;

            var filter = new RecipeFilterDto
            {
                SearchTerm = SearchText,
                DishType = SelectedDishType,
                Status = SelectedStatus,
                Cuisine = SelectedCuisine,
                CategoryId = SelectedCategory?.Id
            };

            var recipes = await _recipeService.SearchAsync(filter);

            Recipes = new ObservableCollection<RecipeListItemDto>(recipes);

            _logger.LogInformation("Search completed, found {Count} recipes", recipes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching recipes");
            SetError("Ошибка поиска: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CreateRecipe()
    {
        _navigationService.NavigateTo("RecipeDetail", null);
    }

    [RelayCommand]
    private void EditRecipe(RecipeListItemDto? recipe)
    {
        if (recipe == null) return;
        _navigationService.NavigateTo("RecipeDetail", recipe.Id);
    }

    [RelayCommand]
    private async Task DeleteRecipeAsync(RecipeListItemDto? recipe)
    {
        if (recipe == null) return;

        if (!_dialogService.ShowConfirm($"Удалить рецепт \"{recipe.Name}\"?"))
            return;

        try
        {
            await _recipeService.DeleteAsync(recipe.Id);
            Recipes.Remove(recipe);
            _dialogService.ShowInfo("Рецепт удален.");
            _logger.LogInformation("Recipe {Id} deleted", recipe.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recipe {Id}", recipe.Id);
            _dialogService.ShowError("Не удалось удалить рецепт: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task PublishRecipeAsync(RecipeDto? recipe)
    {
        if (recipe == null) return;

        try
        {
            await _recipeService.PublishAsync(recipe.Id);
            await SearchAsync();
            _dialogService.ShowInfo("Рецепт опубликован.");
            _logger.LogInformation("Recipe {Id} published", recipe.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing recipe {Id}", recipe.Id);
            _dialogService.ShowError("Не удалось опубликовать рецепт: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task ArchiveRecipeAsync(RecipeDto? recipe)
    {
        if (recipe == null) return;

        if (!_dialogService.ShowConfirm($"Архивировать рецепт \"{recipe.Name}\"?"))
            return;

        try
        {
            await _recipeService.ArchiveAsync(recipe.Id);
            await SearchAsync();
            _dialogService.ShowInfo("Рецепт архивирован.");
            _logger.LogInformation("Recipe {Id} archived", recipe.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving recipe {Id}", recipe.Id);
            _dialogService.ShowError("Не удалось архивировать рецепт: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task RestoreRecipeAsync(RecipeDto? recipe)
    {
        if (recipe == null) return;

        try
        {
            await _recipeService.RestoreAsync(recipe.Id);
            await SearchAsync();
            _dialogService.ShowInfo("Рецепт восстановлен.");
            _logger.LogInformation("Recipe {Id} restored", recipe.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring recipe {Id}", recipe.Id);
            _dialogService.ShowError("Не удалось восстановить рецепт: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task ExportToCsvAsync()
    {
        var filePath = _dialogService.ShowSaveFileDialog(
            "CSV файлы (*.csv)|*.csv", 
            "recipes_export.csv");

        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            // TODO: Implement export service method
            _dialogService.ShowInfo("Функция экспорта в разработке");
            _logger.LogInformation("Export requested but not implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting recipes");
            _dialogService.ShowError("Ошибка экспорта: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task ExportRecipeDetailAsync(RecipeListItemDto? recipe)
    {
        if (recipe == null) return;

        var filePath = _dialogService.ShowSaveFileDialog(
            "CSV файлы (*.csv)|*.csv",
            $"recipe_{recipe.Code}.csv");

        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            // TODO: Implement export service method
            _dialogService.ShowInfo("Функция экспорта в разработке");
            _logger.LogInformation("Recipe {Id} export requested but not implemented", recipe.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting recipe {Id}", recipe.Id);
            _dialogService.ShowError("Ошибка экспорта: " + ex.Message);
        }
    }
}
