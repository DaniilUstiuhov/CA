using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CulinaryAssistant.UI.ViewModels;

public partial class ShoppingListViewModel : ObservableObject
{
    private readonly IShoppingListService _shoppingListService;
    private readonly IExportService _exportService;
    private readonly ILogger<ShoppingListViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ShoppingListDto> _shoppingLists = new();

    [ObservableProperty]
    private ShoppingListDto? _selectedList;

    [ObservableProperty]
    private string _newItemName = string.Empty;

    [ObservableProperty]
    private string _newItemQuantity = "1";

    [ObservableProperty]
    private bool _isLoading;

    public bool HasSelectedList => SelectedList != null;

    public string CompleteButtonText => SelectedList?.IsCompleted == true ? "Открыть" : "Завершить";

    public ShoppingListViewModel(
        IShoppingListService shoppingListService,
        IExportService exportService,
        ILogger<ShoppingListViewModel> logger)
    {
        _shoppingListService = shoppingListService;
        _exportService = exportService;
        _logger = logger;
    }

    public async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading shopping lists");

            var lists = await _shoppingListService.GetAllAsync();
            ShoppingLists = new ObservableCollection<ShoppingListDto>(lists);

            if (SelectedList != null)
            {
                SelectedList = ShoppingLists.FirstOrDefault(l => l.Id == SelectedList.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading shopping lists");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedListChanged(ShoppingListDto? value)
    {
        OnPropertyChanged(nameof(HasSelectedList));
        OnPropertyChanged(nameof(CompleteButtonText));
    }

    [RelayCommand]
    private async Task SelectListAsync(ShoppingListDto list)
    {
        try
        {
            var fullList = await _shoppingListService.GetByIdAsync(list.Id);
            SelectedList = fullList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting shopping list {ListId}", list.Id);
        }
    }

    [RelayCommand]
    private async Task CreateListAsync()
    {
        try
        {
            var name = $"Список покупок {DateTime.Now:dd.MM.yyyy HH:mm}";
            var dto = new ShoppingListCreateDto { Name = name };
            var created = await _shoppingListService.CreateAsync(dto);
            
            ShoppingLists.Insert(0, new ShoppingListDto
            {
                Id = created.Id,
                Name = created.Name,
                IsCompleted = false,
                TotalItems = 0,
                PurchasedItems = 0,
                CompletionPercentage = 0
            });

            await SelectListAsync(ShoppingLists[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shopping list");
        }
    }

    [RelayCommand]
    private async Task DeleteListAsync()
    {
        if (SelectedList == null) return;

        try
        {
            await _shoppingListService.DeleteAsync(SelectedList.Id);
            ShoppingLists.Remove(ShoppingLists.First(l => l.Id == SelectedList.Id));
            SelectedList = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shopping list");
        }
    }

    [RelayCommand]
    private async Task ToggleCompleteAsync()
    {
        if (SelectedList == null) return;

        try
        {
            if (SelectedList.IsCompleted)
            {
                await _shoppingListService.ReopenAsync(SelectedList.Id);
            }
            else
            {
                await _shoppingListService.MarkAsCompletedAsync(SelectedList.Id);
            }

            await LoadAsync();
            if (SelectedList != null)
            {
                SelectedList = ShoppingLists.FirstOrDefault(l => l.Id == SelectedList.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling shopping list completion");
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        if (SelectedList == null || string.IsNullOrWhiteSpace(NewItemName)) return;

        try
        {
            if (!decimal.TryParse(NewItemQuantity, out var quantity))
                quantity = 1;

            var itemDto = new ShoppingItemCreateDto
            {
                Name = NewItemName.Trim(),
                Quantity = quantity,
                Unit = MeasurementUnit.Piece
            };

            await _shoppingListService.AddItemAsync(SelectedList.Id, itemDto);
            
            NewItemName = string.Empty;
            NewItemQuantity = "1";

            // Reload the selected list
            var fullList = await _shoppingListService.GetByIdAsync(SelectedList.Id);
            SelectedList = fullList;

            // Update the list in the sidebar
            var listInCollection = ShoppingLists.FirstOrDefault(l => l.Id == SelectedList!.Id);
            if (listInCollection != null)
            {
                var index = ShoppingLists.IndexOf(listInCollection);
                ShoppingLists[index] = new ShoppingListDto
                {
                    Id = fullList!.Id,
                    Name = fullList.Name,
                    IsCompleted = fullList.IsCompleted,
                    TotalItems = fullList.TotalItems,
                    PurchasedItems = fullList.PurchasedItems,
                    CompletionPercentage = fullList.CompletionPercentage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to shopping list");
        }
    }

    [RelayCommand]
    private async Task RemoveItemAsync(ShoppingItemDto item)
    {
        if (SelectedList == null) return;

        try
        {
            await _shoppingListService.RemoveItemAsync(SelectedList.Id, item.Id);
            
            var fullList = await _shoppingListService.GetByIdAsync(SelectedList.Id);
            SelectedList = fullList;

            await LoadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from shopping list");
        }
    }

    [RelayCommand]
    private async Task ToggleItemAsync(ShoppingItemDto item)
    {
        if (SelectedList == null) return;

        try
        {
            await _shoppingListService.ToggleItemPurchasedAsync(SelectedList.Id, item.Id);
            
            var fullList = await _shoppingListService.GetByIdAsync(SelectedList.Id);
            SelectedList = fullList;

            // Update the list in the sidebar
            var listInCollection = ShoppingLists.FirstOrDefault(l => l.Id == SelectedList!.Id);
            if (listInCollection != null)
            {
                var index = ShoppingLists.IndexOf(listInCollection);
                ShoppingLists[index] = new ShoppingListDto
                {
                    Id = fullList!.Id,
                    Name = fullList.Name,
                    IsCompleted = fullList.IsCompleted,
                    TotalItems = fullList.TotalItems,
                    PurchasedItems = fullList.PurchasedItems,
                    CompletionPercentage = fullList.CompletionPercentage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling item purchase status");
        }
    }

    [RelayCommand]
    private async Task ExportListAsync()
    {
        if (SelectedList == null) return;

        try
        {
            var csvBytes = await _exportService.ExportShoppingListToCsvAsync(SelectedList.Id);
            var fileName = $"shopping_list_{SelectedList.Name}_{DateTime.Now:yyyyMMdd}.csv";
            var filePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                fileName);
            
            await System.IO.File.WriteAllBytesAsync(filePath, csvBytes);
            _logger.LogInformation("Shopping list exported to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting shopping list");
        }
    }
}
