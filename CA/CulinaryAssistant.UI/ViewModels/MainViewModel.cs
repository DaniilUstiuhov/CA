using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CulinaryAssistant.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryAssistant.UI.ViewModels;

/// <summary>
/// Base ViewModel with common functionality
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    protected void ClearError() => ErrorMessage = null;

    protected void SetError(string message) => ErrorMessage = message;
}

/// <summary>
/// Main application ViewModel - manages navigation and main state
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentViewTitle = "Главная";

    [ObservableProperty]
    private bool _isDashboardSelected = true;

    [ObservableProperty]
    private bool _isRecipesSelected;

    [ObservableProperty]
    private bool _isInventorySelected;

    [ObservableProperty]
    private bool _isShoppingSelected;

    public MainViewModel(INavigationService navigationService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        _navigationService.NavigationRequested += OnNavigationRequested;

        // Navigate to Dashboard by default
        Navigate("Dashboard");
    }

    private void OnNavigationRequested(string viewName)
    {
        Navigate(viewName);
    }

    [RelayCommand]
    private async Task Navigate(string destination)
    {
        switch (destination)
        {
            case "Dashboard":
                var dashboardVm = _serviceProvider.GetRequiredService<DashboardViewModel>();
                await dashboardVm.LoadAsync();
                CurrentView = dashboardVm;
                CurrentViewTitle = "📊 Главная панель";
                UpdateSelection("Dashboard");
                break;
            case "Recipes":
                var recipesVm = _serviceProvider.GetRequiredService<RecipeListViewModel>();
                await recipesVm.LoadAsync();
                CurrentView = recipesVm;
                CurrentViewTitle = "🍳 Рецепты";
                UpdateSelection("Recipes");
                break;
            case "RecipeDetail":
                var recipeDetailVm = _serviceProvider.GetRequiredService<RecipeDetailViewModel>();
                CurrentView = recipeDetailVm;
                CurrentViewTitle = "📝 Редактирование рецепта";
                UpdateSelection("Recipes");
                break;
            case "Inventory":
                var inventoryVm = _serviceProvider.GetRequiredService<InventoryViewModel>();
                await inventoryVm.LoadAsync();
                CurrentView = inventoryVm;
                CurrentViewTitle = "🏪 Склад продуктов";
                UpdateSelection("Inventory");
                break;
            case "Shopping":
                var shoppingVm = _serviceProvider.GetRequiredService<ShoppingListViewModel>();
                await shoppingVm.LoadAsync();
                CurrentView = shoppingVm;
                CurrentViewTitle = "🛒 Списки покупок";
                UpdateSelection("Shopping");
                break;
        }
    }

    private void UpdateSelection(string view)
    {
        IsDashboardSelected = view == "Dashboard";
        IsRecipesSelected = view == "Recipes";
        IsInventorySelected = view == "Inventory";
        IsShoppingSelected = view == "Shopping";
    }
}
