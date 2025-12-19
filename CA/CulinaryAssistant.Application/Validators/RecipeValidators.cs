using CulinaryAssistant.Application.DTOs;
using FluentValidation;

namespace CulinaryAssistant.Application.Validators;

/// <summary>
/// Валидатор для создания рецепта
/// </summary>
public class RecipeCreateDtoValidator : AbstractValidator<RecipeCreateDto>
{
    public RecipeCreateDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Код рецепта обязателен")
            .MaximumLength(20).WithMessage("Код рецепта не может превышать 20 символов")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("Код может содержать только буквы, цифры, дефис и подчёркивание");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название рецепта обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Cuisine)
            .NotEmpty().WithMessage("Кухня обязательна")
            .MaximumLength(100).WithMessage("Название кухни не может превышать 100 символов");

        RuleFor(x => x.CookingTimeMinutes)
            .GreaterThan(0).WithMessage("Время приготовления должно быть положительным")
            .LessThanOrEqualTo(1440).WithMessage("Время приготовления не может превышать 24 часа");

        RuleFor(x => x.Servings)
            .GreaterThan(0).WithMessage("Количество порций должно быть положительным")
            .LessThanOrEqualTo(100).WithMessage("Количество порций не может превышать 100");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не может превышать 2000 символов");

        RuleFor(x => x.Instructions)
            .MaximumLength(10000).WithMessage("Инструкции не могут превышать 10000 символов");
    }
}

/// <summary>
/// Валидатор для обновления рецепта
/// </summary>
public class RecipeUpdateDtoValidator : AbstractValidator<RecipeUpdateDto>
{
    public RecipeUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Некорректный идентификатор рецепта");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Код рецепта обязателен")
            .MaximumLength(20).WithMessage("Код рецепта не может превышать 20 символов")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("Код может содержать только буквы, цифры, дефис и подчёркивание");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название рецепта обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Cuisine)
            .NotEmpty().WithMessage("Кухня обязательна")
            .MaximumLength(100).WithMessage("Название кухни не может превышать 100 символов");

        RuleFor(x => x.CookingTimeMinutes)
            .GreaterThan(0).WithMessage("Время приготовления должно быть положительным")
            .LessThanOrEqualTo(1440).WithMessage("Время приготовления не может превышать 24 часа");

        RuleFor(x => x.Servings)
            .GreaterThan(0).WithMessage("Количество порций должно быть положительным")
            .LessThanOrEqualTo(100).WithMessage("Количество порций не может превышать 100");
    }
}

/// <summary>
/// Валидатор для создания ингредиента
/// </summary>
public class RecipeIngredientCreateDtoValidator : AbstractValidator<RecipeIngredientCreateDto>
{
    public RecipeIngredientCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название ингредиента обязательно")
            .MaximumLength(200).WithMessage("Название не может превышать 200 символов");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Количество должно быть положительным");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Примечания не могут превышать 500 символов");
    }
}
