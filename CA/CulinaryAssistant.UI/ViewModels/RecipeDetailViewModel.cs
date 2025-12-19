using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Domain.Enums;
using CulinaryAssistant.UI.Services;
using Microsoft.Extensions.Logging;

namespace CulinaryAssistant.UI.ViewModels;

/// <summary>
/// Recipe detail ViewModel - add/edit recipe with ingredients
/// </summary>
public partial class RecipeDetailViewModel : ViewModelBase
{
    private readonly IRecipeService _recipeService;
    private readonly ICategoryService _categoryService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<RecipeDetailViewModel> _logger;

    private int? _recipeId;

    [ObservableProperty]
    private bool _isNewRecipe = true;

    [ObservableProperty]
    private bool _canEdit = true;

    [ObservableProperty]
    private string _pageTitle = "Новый рецепт";

    // Recipe fields
    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _instructions = string.Empty;

    [ObservableProperty]
    private DishType _dishType = DishType.MainCourse;

    [ObservableProperty]
    private string _cuisine = string.Empty;

    [ObservableProperty]
    private int _cookingTimeMinutes;

    [ObservableProperty]
    private int _servings = 4;

    [ObservableProperty]
    private RecipeStatus _status = RecipeStatus.Draft;

    // Ingredients
    [ObservableProperty]
    private ObservableCollection<RecipeIngredientDto> _ingredients = new();

    [ObservableProperty]
    private RecipeIngredientDto? _selectedIngredient;

    // New ingredient form
    [ObservableProperty]
    private string _newIngredientName = string.Empty;

    [ObservableProperty]
    private double _newIngredientAmount = 1;

    [ObservableProperty]
    private MeasurementUnit _newIngredientUnit = MeasurementUnit.Gram;

    [ObservableProperty]
    private bool _newIngredientIsOptional;

    [ObservableProperty]
    private string _newIngredientNotes = string.Empty;

    // Categories
    [ObservableProperty]
    private ObservableCollection<CategoryDto> _allCategories = new();

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _selectedCategories = new();

    [ObservableProperty]
    private CategoryDto? _categoryToAdd;

    // Enum values for ComboBoxes
    public IEnumerable<DishType> DishTypes => Enum.GetValues<DishType>();
    public IEnumerable<MeasurementUnit> MeasurementUnits => Enum.GetValues<MeasurementUnit>();

    public RecipeDetailViewModel(
        IRecipeService recipeService,
        ICategoryService categoryService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<RecipeDetailViewModel> logger)
    {
        _recipeService = recipeService;
        _categoryService = categoryService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;

        // Check if we have a recipe ID to load
        var parameter = navigationService.CurrentParameter;
        if (parameter is int id)
        {
            _recipeId = id;
            IsNewRecipe = false;
            PageTitle = "Редактировать рецепт";
        }

        LoadDataCommand.Execute(null);
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
            AllCategories = new ObservableCollection<CategoryDto>(categories);

            // Load existing recipe if editing
            if (_recipeId.HasValue)
            {
                var recipe = await _recipeService.GetByIdAsync(_recipeId.Value);
                if (recipe != null)
                {
                    Code = recipe.Code;
                    Name = recipe.Name;
                    Description = recipe.Description ?? string.Empty;
                    Instructions = recipe.Instructions ?? string.Empty;
                    DishType = recipe.DishType;
                    Cuisine = recipe.Cuisine ?? string.Empty;
                    CookingTimeMinutes = recipe.CookingTimeMinutes;
                    Servings = recipe.Servings;
                    Status = recipe.Status;
                    CanEdit = recipe.Status == Domain.Enums.RecipeStatus.Draft;
                    PageTitle = $"Редактировать: {recipe.Name}";

                    Ingredients = new ObservableCollection<RecipeIngredientDto>(recipe.Ingredients);
                    SelectedCategories = new ObservableCollection<CategoryDto>(recipe.Categories);
                }
            }

            _logger.LogInformation("Recipe detail loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recipe detail");
            SetError("Ошибка загрузки: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddIngredient()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientName))
        {
            _dialogService.ShowWarning("Введите название ингредиента.");
            return;
        }

        var ingredient = new RecipeIngredientDto
        {
            Name = NewIngredientName.Trim(),
            Amount = NewIngredientAmount,
            Unit = NewIngredientUnit,
            IsOptional = NewIngredientIsOptional,
            Notes = NewIngredientNotes?.Trim()
        };

        Ingredients.Add(ingredient);

        // Clear form
        NewIngredientName = string.Empty;
        NewIngredientAmount = 1;
        NewIngredientUnit = MeasurementUnit.Gram;
        NewIngredientIsOptional = false;
        NewIngredientNotes = string.Empty;
    }

    [RelayCommand]
    private void RemoveIngredient(RecipeIngredientDto? ingredient)
    {
        if (ingredient != null)
        {
            Ingredients.Remove(ingredient);
        }
    }

    [RelayCommand]
    private void AddCategory()
    {
        if (CategoryToAdd == null) return;
        if (SelectedCategories.Any(c => c.Id == CategoryToAdd.Id)) return;

        SelectedCategories.Add(CategoryToAdd);
        CategoryToAdd = null;
    }

    [RelayCommand]
    private void RemoveCategory(CategoryDto? category)
    {
        if (category != null)
        {
            SelectedCategories.Remove(category);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;

        try
        {
            IsLoading = true;

            if (IsNewRecipe)
            {
                var createDto = new RecipeCreateDto
                {
                    Code = Code.Trim(),
                    Name = Name.Trim(),
                    Description = Description?.Trim(),
                    Instructions = Instructions?.Trim(),
                    DishType = DishType,
                    Cuisine = Cuisine?.Trim(),
                    CookingTimeMinutes = CookingTimeMinutes > 0 ? CookingTimeMinutes : 0,
                    Servings = Servings > 0 ? Servings : 4
                };

                var result = await _recipeService.CreateAsync(createDto);
                _recipeId = result.Id;
                IsNewRecipe = false;
                _dialogService.ShowInfo("Рецепт создан.");
                _logger.LogInformation("Recipe created with ID {Id}", result.Id);
            }
            else
            {
                var updateDto = new RecipeUpdateDto
                {
                    Id = _recipeId!.Value,
                    Code = Code.Trim(),
                    Name = Name.Trim(),
                    Description = Description?.Trim(),
                    Instructions = Instructions?.Trim(),
                    DishType = DishType,
                    Cuisine = Cuisine?.Trim(),
                    CookingTimeMinutes = CookingTimeMinutes > 0 ? CookingTimeMinutes : 0,
                    Servings = Servings > 0 ? Servings : 4
                };

                await _recipeService.UpdateAsync(updateDto);

                // TODO: Implement ingredient and category updates using Add/Remove methods

                _dialogService.ShowInfo("Рецепт сохранен.");
                _logger.LogInformation("Recipe {Id} updated", _recipeId);
            }

            _navigationService.NavigateTo("Recipes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving recipe");
            _dialogService.ShowError("Ошибка сохранения: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateTo("Recipes");
    }

    [RelayCommand]
    private async Task PublishAsync()
    {
        if (_recipeId == null) return;

        try
        {
            await _recipeService.PublishAsync(_recipeId.Value);
            Status = RecipeStatus.Published;
            CanEdit = false;
            _dialogService.ShowInfo("Рецепт опубликован.");
            _logger.LogInformation("Recipe {Id} published", _recipeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing recipe");
            _dialogService.ShowError("Не удалось опубликовать: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task ReturnToDraftAsync()
    {
        if (_recipeId == null) return;

        try
        {
            await _recipeService.ReturnToDraftAsync(_recipeId.Value);
            Status = RecipeStatus.Draft;
            CanEdit = true;
            _dialogService.ShowInfo("Рецепт возвращен в черновики.");
            _logger.LogInformation("Recipe {Id} returned to draft", _recipeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning recipe to draft");
            _dialogService.ShowError("Не удалось вернуть в черновики: " + ex.Message);
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(Code))
        {
            _dialogService.ShowWarning("Введите код рецепта.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            _dialogService.ShowWarning("Введите название рецепта.");
            return false;
        }

        return true;
    }
}
