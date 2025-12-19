using CulinaryAssistant.Application.DTOs;
using FluentValidation;

namespace CulinaryAssistant.Application.Validators;

/// <summary>
/// Валидатор для создания списка покупок
/// </summary>
public class ShoppingListCreateDtoValidator : AbstractValidator<ShoppingListCreateDto>
{
    public ShoppingListCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название списка обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов");
    }
}

/// <summary>
/// Валидатор для добавления элемента в список покупок
/// </summary>
public class ShoppingItemCreateDtoValidator : AbstractValidator<ShoppingItemCreateDto>
{
    public ShoppingItemCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название товара обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Количество должно быть положительным");

        RuleFor(x => x.EstimatedPrice)
            .GreaterThanOrEqualTo(0).When(x => x.EstimatedPrice.HasValue)
            .WithMessage("Цена не может быть отрицательной");

        RuleFor(x => x.PreferredStore)
            .MaximumLength(100).WithMessage("Название магазина не может превышать 100 символов");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Примечания не могут превышать 500 символов");
    }
}

/// <summary>
/// Валидатор для создания категории
/// </summary>
public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название категории обязательно")
            .MaximumLength(100).WithMessage("Название не может превышать 100 символов");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Описание не может превышать 500 символов");
    }
}
