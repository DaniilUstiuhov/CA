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
/// Inventory ViewModel - manage inventory items with expiration tracking
/// </summary>
public partial class InventoryViewModel : ViewModelBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IExportService _exportService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<InventoryViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<InventoryItemDto> _items = new();

    [ObservableProperty]
    private InventoryItemDto? _selectedItem;

    [ObservableProperty]
    private ObservableCollection<string> _storageLocations = new();

    // Filters
    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private string? _selectedStorageLocation;

    [ObservableProperty]
    private bool _showExpiredOnly;

    [ObservableProperty]
    private bool _showExpiringSoonOnly;

    // Edit form
    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int? _editingItemId;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private double _editQuantity = 1;

    [ObservableProperty]
    private MeasurementUnit _editUnit = MeasurementUnit.Piece;

    [ObservableProperty]
    private DateTime? _editExpirationDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private string _editStorageLocation = string.Empty;

    public IEnumerable<MeasurementUnit> MeasurementUnits => Enum.GetValues<MeasurementUnit>();

    public int ExpiredCount => Items.Count(i => i.IsExpired);
    public int ExpiringSoonCount => Items.Count(i => i.IsExpiringSoon && !i.IsExpired);

    public ObservableCollection<InventoryItemDto> FilteredItems => Items;

    public InventoryViewModel(
        IInventoryService inventoryService,
        IExportService exportService,
        IDialogService dialogService,
        ILogger<InventoryViewModel> logger)
    {
        _inventoryService = inventoryService;
        _exportService = exportService;
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
        SelectedStorageLocation = null;
        ShowExpiredOnly = false;
        ShowExpiringSoonOnly = false;
        await SearchAsync();
    }

    [RelayCommand]
    private void AddItem()
    {
        NewItem();
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        await ExportToCsvAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var locations = await _inventoryService.GetAllStorageLocationsAsync();
            StorageLocations = new ObservableCollection<string>(locations);

            await SearchAsync();

            _logger.LogInformation("Inventory data loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading inventory data");
            SetError("Ошибка загрузки: " + ex.Message);
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

            var items = await _inventoryService.SearchAsync(
                searchText: SearchText,
                storageLocation: SelectedStorageLocation,
                expiredOnly: ShowExpiredOnly ? true : null,
                expiringSoon: ShowExpiringSoonOnly ? true : null);

            Items = new ObservableCollection<InventoryItemDto>(items);

            _logger.LogInformation("Search completed, found {Count} items", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching inventory");
            SetError("Ошибка поиска: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NewItem()
    {
        IsEditMode = true;
        EditingItemId = null;
        EditName = string.Empty;
        EditQuantity = 1;
        EditUnit = MeasurementUnit.Piece;
        EditExpirationDate = DateTime.Today.AddDays(7);
        EditStorageLocation = string.Empty;
    }

    [RelayCommand]
    private void EditItem(InventoryItemDto? item)
    {
        if (item == null) return;

        IsEditMode = true;
        EditingItemId = item.Id;
        EditName = item.Name;
        EditQuantity = item.Quantity;
        EditUnit = item.Unit;
        EditExpirationDate = item.ExpirationDate;
        EditStorageLocation = item.StorageLocation ?? string.Empty;
    }

    [RelayCommand]
    private async Task SaveItemAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            _dialogService.ShowWarning("Введите название продукта.");
            return;
        }

        try
        {
            IsLoading = true;

            if (EditingItemId == null)
            {
                // Create new
                var createDto = new InventoryItemCreateDto
                {
                    Name = EditName.Trim(),
                    Quantity = EditQuantity,
                    Unit = EditUnit,
                    ExpirationDate = EditExpirationDate,
                    StorageLocation = string.IsNullOrWhiteSpace(EditStorageLocation) ? null : EditStorageLocation.Trim()
                };

                await _inventoryService.CreateAsync(createDto);
                _dialogService.ShowInfo("Продукт добавлен.");
                _logger.LogInformation("Inventory item created: {Name}", createDto.Name);
            }
            else
            {
                // Update existing
                var updateDto = new InventoryItemUpdateDto
                {
                    Id = EditingItemId.Value,
                    Name = EditName.Trim(),
                    Quantity = EditQuantity,
                    Unit = EditUnit,
                    ExpirationDate = EditExpirationDate,
                    StorageLocation = string.IsNullOrWhiteSpace(EditStorageLocation) ? null : EditStorageLocation.Trim()
                };

                await _inventoryService.UpdateAsync(updateDto);
                _dialogService.ShowInfo("Продукт обновлен.");
                _logger.LogInformation("Inventory item {Id} updated", EditingItemId);
            }

            CancelEdit();
            await SearchAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving inventory item");
            _dialogService.ShowError("Ошибка сохранения: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditMode = false;
        EditingItemId = null;
    }

    [RelayCommand]
    private async Task DeleteItemAsync(InventoryItemDto? item)
    {
        if (item == null) return;

        if (!_dialogService.ShowConfirm($"Удалить \"{item.Name}\"?"))
            return;

        try
        {
            await _inventoryService.DeleteAsync(item.Id);
            Items.Remove(item);
            _dialogService.ShowInfo("Продукт удален.");
            _logger.LogInformation("Inventory item {Id} deleted", item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory item {Id}", item.Id);
            _dialogService.ShowError("Ошибка удаления: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task UseItemAsync(InventoryItemDto? item)
    {
        if (item == null) return;

        try
        {
            // Use 1 unit
            await _inventoryService.UseItemAsync(item.Id, 1);
            await SearchAsync();
            _logger.LogInformation("Used 1 unit of item {Id}", item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using item {Id}", item.Id);
            _dialogService.ShowError("Ошибка: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task ReplenishItemAsync(InventoryItemDto? item)
    {
        if (item == null) return;

        try
        {
            // Add 1 unit
            await _inventoryService.ReplenishItemAsync(item.Id, 1);
            await SearchAsync();
            _logger.LogInformation("Replenished 1 unit of item {Id}", item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replenishing item {Id}", item.Id);
            _dialogService.ShowError("Ошибка: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task ExportToCsvAsync()
    {
        var filePath = _dialogService.ShowSaveFileDialog(
            "CSV файлы (*.csv)|*.csv",
            "inventory_export.csv");

        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            var csv = await _exportService.ExportInventoryAsync();
            await File.WriteAllTextAsync(filePath, csv);
            _dialogService.ShowInfo($"Экспорт завершен: {filePath}");
            _logger.LogInformation("Inventory exported to {Path}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting inventory");
            _dialogService.ShowError("Ошибка экспорта: " + ex.Message);
        }
    }
}
