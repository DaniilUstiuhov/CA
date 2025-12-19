using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Exceptions;
using CulinaryAssistant.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CulinaryAssistant.Application.Services;

/// <summary>
/// Сервис списков покупок (Application Layer)
/// </summary>
public class ShoppingListService : IShoppingListService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShoppingListService> _logger;
    private readonly IValidator<ShoppingListCreateDto> _listValidator;
    private readonly IValidator<ShoppingItemCreateDto> _itemValidator;

    public ShoppingListService(
        IUnitOfWork unitOfWork,
        ILogger<ShoppingListService> logger,
        IValidator<ShoppingListCreateDto> listValidator,
        IValidator<ShoppingItemCreateDto> itemValidator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _listValidator = listValidator;
        _itemValidator = itemValidator;
    }

    public async Task<ShoppingListDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting shopping list by ID: {Id}", id);
        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(id, cancellationToken);
        return list != null ? MapToDto(list) : null;
    }

    public async Task<IReadOnlyList<ShoppingListSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all shopping lists");
        var lists = await _unitOfWork.ShoppingLists.GetAllAsync(cancellationToken);
        return lists.Select(MapToSummaryDto).ToList();
    }

    public async Task<IReadOnlyList<ShoppingListSummaryDto>> GetActiveListsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting active shopping lists");
        var lists = await _unitOfWork.ShoppingLists.GetActiveListsAsync(cancellationToken);
        return lists.Select(MapToSummaryDto).ToList();
    }

    public async Task<IReadOnlyList<ShoppingListSummaryDto>> GetCompletedListsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting completed shopping lists");
        var lists = await _unitOfWork.ShoppingLists.GetCompletedListsAsync(cancellationToken);
        return lists.Select(MapToSummaryDto).ToList();
    }

    public async Task<ShoppingListDto> CreateAsync(ShoppingListCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new shopping list: {Name}", dto.Name);

        var validationResult = await _listValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var list = new ShoppingList(dto.Name, dto.Description);

        await _unitOfWork.ShoppingLists.AddAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shopping list created with ID: {Id}", list.Id);
        return MapToDto(list);
    }

    public async Task<ShoppingListDto> UpdateAsync(int id, ShoppingListCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating shopping list: {Id}", id);

        var validationResult = await _listValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(id, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {id} не найден");

        list.SetName(dto.Name);
        list.SetDescription(dto.Description);

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shopping list updated: {Id}", list.Id);
        return MapToDto(list);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting shopping list: {Id}", id);

        var list = await _unitOfWork.ShoppingLists.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {id} не найден");

        await _unitOfWork.ShoppingLists.DeleteAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shopping list deleted: {Id}", id);
    }

    public async Task<ShoppingListDto> AddItemAsync(int listId, ShoppingItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding item to shopping list: {ListId}", listId);

        var validationResult = await _itemValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new DomainException(string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(listId, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {listId} не найден");

        list.AddItem(dto.Name, dto.Quantity, dto.Unit, dto.EstimatedPrice, dto.PreferredStore, dto.Notes);

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(list);
    }

    public async Task<ShoppingListDto> RemoveItemAsync(int listId, int itemId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing item {ItemId} from shopping list: {ListId}", itemId, listId);

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(listId, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {listId} не найден");

        var item = list.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Товар с ID {itemId} не найден в списке");

        list.RemoveItem(item);

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(list);
    }

    public async Task<ShoppingListDto> MarkItemPurchasedAsync(int listId, int itemId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking item {ItemId} as purchased in list: {ListId}", itemId, listId);

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(listId, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {listId} не найден");

        var item = list.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Товар с ID {itemId} не найден в списке");

        item.MarkAsPurchased();

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(list);
    }

    public async Task<ShoppingListDto> MarkItemNotPurchasedAsync(int listId, int itemId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking item {ItemId} as not purchased in list: {ListId}", itemId, listId);

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(listId, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {listId} не найден");

        var item = list.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Товар с ID {itemId} не найден в списке");

        item.MarkAsNotPurchased();

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(list);
    }

    public async Task<ShoppingListDto> MarkAsCompletedAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking shopping list as completed: {Id}", id);

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(id, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {id} не найден");

        list.MarkAsCompleted();

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(list);
    }

    public async Task<ShoppingListDto> ReopenAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reopening shopping list: {Id}", id);

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(id, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {id} не найден");

        list.Reopen();

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(list);
    }

    public async Task<ShoppingListDto> ClearPurchasedItemsAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing purchased items from shopping list: {Id}", id);

        var list = await _unitOfWork.ShoppingLists.GetWithItemsAsync(id, cancellationToken)
            ?? throw new DomainException($"Список покупок с ID {id} не найден");

        list.ClearPurchasedItems();

        await _unitOfWork.ShoppingLists.UpdateAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(list);
    }

    private static ShoppingListDto MapToDto(ShoppingList list) => new()
    {
        Id = list.Id,
        Name = list.Name,
        Description = list.Description,
        IsCompleted = list.IsCompleted,
        CompletedAt = list.CompletedAt,
        TotalItems = list.TotalItems,
        PurchasedItems = list.PurchasedItems,
        TotalEstimatedPrice = list.TotalEstimatedPrice,
        CompletionPercentage = list.CompletionPercentage,
        CreatedAt = list.CreatedAt,
        Items = list.Items.Select(i => new ShoppingItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Quantity = i.Quantity,
            Unit = i.Unit,
            IsPurchased = i.IsPurchased,
            PurchasedAt = i.PurchasedAt,
            EstimatedPrice = i.EstimatedPrice,
            PreferredStore = i.PreferredStore,
            Notes = i.Notes
        }).ToList()
    };

    private static ShoppingListSummaryDto MapToSummaryDto(ShoppingList list) => new()
    {
        Id = list.Id,
        Name = list.Name,
        IsCompleted = list.IsCompleted,
        TotalItems = list.TotalItems,
        PurchasedItems = list.PurchasedItems,
        CompletionPercentage = list.CompletionPercentage,
        CreatedAt = list.CreatedAt
    };
}
