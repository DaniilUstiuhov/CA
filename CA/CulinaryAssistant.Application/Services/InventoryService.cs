using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Exceptions;
using CulinaryAssistant.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CulinaryAssistant.Application.Services;

/// <summary>
/// Сервис инвентаря (Application Layer)
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InventoryService> _logger;
    private readonly IValidator<InventoryItemCreateDto> _createValidator;
    private readonly IValidator<InventoryItemUpdateDto> _updateValidator;

    public InventoryService(
        IUnitOfWork unitOfWork,
        ILogger<InventoryService> logger,
        IValidator<InventoryItemCreateDto> createValidator,
        IValidator<InventoryItemUpdateDto> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<InventoryItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting inventory item by ID: {Id}", id);
        var item = await _unitOfWork.Inventory.GetByIdAsync(id, cancellationToken);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all inventory items");
        var items = await _unitOfWork.Inventory.GetAllAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<InventoryItemDto>> SearchAsync(InventoryFilterDto filter, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching inventory with filter: {@Filter}", filter);
        
        IReadOnlyList<InventoryItem> items;
        
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            items = await _unitOfWork.Inventory.SearchByNameAsync(filter.SearchTerm, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(filter.StorageLocation))
        {
            items = await _unitOfWork.Inventory.GetByStorageLocationAsync(filter.StorageLocation, cancellationToken);
        }
        else
        {
            items = await _unitOfWork.Inventory.GetAllAsync(cancellationToken);
        }

        var result = items.AsEnumerable();

        if (filter.ShowExpired == true)
            result = result.Where(i => i.IsExpired);
        else if (filter.ShowExpired == false)
            result = result.Where(i => !i.IsExpired);

        if (filter.ShowExpiringSoon == true)
            result = result.Where(i => i.IsExpiringSoon);

        return result.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetExpiredItemsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting expired inventory items");
        var items = await _unitOfWork.Inventory.GetExpiredItemsAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetExpiringSoonItemsAsync(int daysThreshold = 3, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting expiring soon inventory items (threshold: {Days} days)", daysThreshold);
        var items = await _unitOfWork.Inventory.GetExpiringSoonItemsAsync(daysThreshold, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<InventoryItemDto> CreateAsync(InventoryItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new inventory item: {Name}", dto.Name);

        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var item = new InventoryItem(dto.Name, dto.Quantity, dto.Unit, dto.ExpirationDate, dto.StorageLocation);

        await _unitOfWork.Inventory.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory item created with ID: {Id}", item.Id);
        return MapToDto(item);
    }

    public async Task<InventoryItemDto> UpdateAsync(InventoryItemUpdateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating inventory item: {Id}", dto.Id);

        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var item = await _unitOfWork.Inventory.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new DomainException($"Продукт с ID {dto.Id} не найден");

        item.SetName(dto.Name);
        item.SetQuantity(dto.Quantity);
        item.SetUnit(dto.Unit);
        item.SetExpirationDate(dto.ExpirationDate);
        item.SetStorageLocation(dto.StorageLocation);

        await _unitOfWork.Inventory.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory item updated: {Id}", item.Id);
        return MapToDto(item);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting inventory item: {Id}", id);

        var item = await _unitOfWork.Inventory.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Продукт с ID {id} не найден");

        await _unitOfWork.Inventory.DeleteAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory item deleted: {Id}", id);
    }

    public async Task UseItemAsync(int id, double amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Using {Amount} of inventory item: {Id}", amount, id);

        var item = await _unitOfWork.Inventory.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Продукт с ID {id} не найден");

        item.Use(amount);
        await _unitOfWork.Inventory.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory item used: {Id}, remaining: {Quantity}", id, item.Quantity);
    }

    public async Task ReplenishItemAsync(int id, double amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Replenishing {Amount} to inventory item: {Id}", amount, id);

        var item = await _unitOfWork.Inventory.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Продукт с ID {id} не найден");

        item.Replenish(amount);
        await _unitOfWork.Inventory.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory item replenished: {Id}, new quantity: {Quantity}", id, item.Quantity);
    }

    private static InventoryItemDto MapToDto(InventoryItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Quantity = item.Quantity,
        Unit = item.Unit,
        ExpirationDate = item.ExpirationDate,
        StorageLocation = item.StorageLocation,
        IsExpired = item.IsExpired,
        IsExpiringSoon = item.IsExpiringSoon,
        DaysUntilExpiration = item.DaysUntilExpiration,
        CreatedAt = item.CreatedAt
    };
}
