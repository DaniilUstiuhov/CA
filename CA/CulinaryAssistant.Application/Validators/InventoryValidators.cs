using CulinaryAssistant.Application.DTOs;
using FluentValidation;

namespace CulinaryAssistant.Application.Validators;

/// <summary>
/// Валидатор для создания элемента инвентаря
/// </summary>
public class InventoryItemCreateDtoValidator : AbstractValidator<InventoryItemCreateDto>
{
    public InventoryItemCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название продукта обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Количество не может быть отрицательным");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty().WithMessage("Срок годности обязателен");

        RuleFor(x => x.StorageLocation)
            .MaximumLength(100).WithMessage("Место хранения не может превышать 100 символов");
    }
}

/// <summary>
/// Валидатор для обновления элемента инвентаря
/// </summary>
public class InventoryItemUpdateDtoValidator : AbstractValidator<InventoryItemUpdateDto>
{
    public InventoryItemUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Некорректный идентификатор");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название продукта обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Количество не может быть отрицательным");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty().WithMessage("Срок годности обязателен");

        RuleFor(x => x.StorageLocation)
            .MaximumLength(100).WithMessage("Место хранения не может превышать 100 символов");
    }
}
