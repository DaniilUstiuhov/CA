using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.UI.Services;
using Microsoft.Extensions.Logging;

namespace CulinaryAssistant.UI.ViewModels;

/// <summary>
/// Dashboard ViewModel - shows aggregated statistics
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private DashboardDto? _dashboardData;

    [ObservableProperty]
    private int _totalRecipes;

    [ObservableProperty]
    private int _publishedRecipes;

    [ObservableProperty]
    private int _draftRecipes;

    [ObservableProperty]
    private int _totalInventoryItems;

    [ObservableProperty]
    private int _expiredItems;

    [ObservableProperty]
    private int _expiringSoonItems;

    [ObservableProperty]
    private int _activeShoppingLists;

    [ObservableProperty]
    private int _totalShoppingItems;

    public DashboardViewModel(
        IDashboardService dashboardService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<DashboardViewModel> logger)
    {
        _dashboardService = dashboardService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;
    }

    public DashboardDto? Dashboard => DashboardData;

    public async Task LoadAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            DashboardData = await _dashboardService.GetDashboardDataAsync();

            TotalRecipes = DashboardData.TotalRecipes;
            PublishedRecipes = DashboardData.PublishedRecipes;
            DraftRecipes = DashboardData.DraftRecipes;
            TotalInventoryItems = DashboardData.TotalInventoryItems;
            ExpiredItems = DashboardData.ExpiredItems;
            ExpiringSoonItems = DashboardData.ExpiringSoonItems;
            ActiveShoppingLists = DashboardData.ActiveShoppingLists;
            TotalShoppingItems = DashboardData.TotalShoppingItems;

            _logger.LogInformation("Dashboard data loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            SetError("Ошибка загрузки данных: " + ex.Message);
            _dialogService.ShowError("Не удалось загрузить данные панели.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoToRecipes()
    {
        _navigationService.NavigateTo("Recipes");
    }

    [RelayCommand]
    private void GoToInventory()
    {
        _navigationService.NavigateTo("Inventory");
    }

    [RelayCommand]
    private void GoToShopping()
    {
        _navigationService.NavigateTo("Shopping");
    }

    [RelayCommand]
    private void CreateNewRecipe()
    {
        _navigationService.NavigateTo("RecipeDetail", null);
    }
}
