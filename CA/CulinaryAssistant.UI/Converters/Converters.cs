using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CulinaryAssistant.Domain.Enums;

namespace CulinaryAssistant.UI.Converters;

/// <summary>
/// Bool to Visibility converter
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}

/// <summary>
/// Inverse Bool to Visibility converter
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Collapsed;
    }
}

/// <summary>
/// Null to Visibility converter
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Enum to string converter
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        return value switch
        {
            RecipeStatus status => status switch
            {
                RecipeStatus.Draft => "Черновик",
                RecipeStatus.Published => "Опубликован",
                RecipeStatus.Archived => "Архив",
                _ => status.ToString()
            },
            DishType dishType => dishType switch
            {
                DishType.FirstCourse => "Первое блюдо",
                DishType.MainCourse => "Второе блюдо",
                DishType.Salad => "Салат",
                DishType.Dessert => "Десерт",
                DishType.Beverage => "Напиток",
                DishType.Appetizer => "Закуска",
                _ => dishType.ToString()
            },
            MeasurementUnit unit => unit switch
            {
                MeasurementUnit.Piece => "шт",
                MeasurementUnit.Gram => "г",
                MeasurementUnit.Kilogram => "кг",
                MeasurementUnit.Milliliter => "мл",
                MeasurementUnit.Liter => "л",
                MeasurementUnit.Tablespoon => "ст.л.",
                MeasurementUnit.Teaspoon => "ч.л.",
                MeasurementUnit.Package => "уп.",
                MeasurementUnit.Cup => "стакан",
                _ => unit.ToString()
            },
            _ => value.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Recipe status to color converter
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RecipeStatus status)
        {
            return status switch
            {
                RecipeStatus.Draft => new SolidColorBrush(Color.FromRgb(255, 185, 0)),     // Warning Yellow
                RecipeStatus.Published => new SolidColorBrush(Color.FromRgb(16, 137, 62)), // Success Green
                RecipeStatus.Archived => new SolidColorBrush(Color.FromRgb(96, 94, 92)),   // Secondary Gray
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Date to string converter
/// </summary>
public class DateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            if (parameter is string format)
                return date.ToString(format);
            
            return date.ToString("dd.MM.yyyy");
        }
        
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToString("dd.MM.yyyy");
        }
        
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && DateTime.TryParse(s, out var date))
            return date;
        return DateTime.Now;
    }
}

/// <summary>
/// Bool to string converter (Да/Нет)
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            if (parameter is string customValues && customValues.Contains('/'))
            {
                var parts = customValues.Split('/');
                return b ? parts[0] : parts[1];
            }
            return b ? "Да" : "Нет";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Expiration status converter
/// </summary>
public class ExpirationStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime expDate)
        {
            var daysUntil = (expDate.Date - DateTime.Today).Days;
            
            if (targetType == typeof(Brush) || targetType == typeof(SolidColorBrush))
            {
                if (daysUntil < 0)
                    return new SolidColorBrush(Color.FromRgb(196, 43, 28));  // Red - expired
                if (daysUntil <= 3)
                    return new SolidColorBrush(Color.FromRgb(255, 185, 0));  // Yellow - expiring soon
                return new SolidColorBrush(Color.FromRgb(16, 137, 62));       // Green - OK
            }
            
            if (daysUntil < 0)
                return $"Просрочено ({Math.Abs(daysUntil)} дн.)";
            if (daysUntil == 0)
                return "Истекает сегодня!";
            if (daysUntil <= 3)
                return $"Истекает через {daysUntil} дн.";
            return $"{daysUntil} дн.";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Shopping list completion percentage to color
/// </summary>
public class CompletionToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            if (percentage >= 100)
                return new SolidColorBrush(Color.FromRgb(16, 137, 62));   // Green - complete
            if (percentage >= 50)
                return new SolidColorBrush(Color.FromRgb(0, 120, 212));   // Blue - in progress
            return new SolidColorBrush(Color.FromRgb(96, 94, 92));        // Gray - just started
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Expiration to color converter for inventory items
/// </summary>
public class ExpirationToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Application.DTOs.InventoryItemDto item)
        {
            if (item.IsExpired)
                return new SolidColorBrush(Color.FromRgb(196, 43, 28));   // Red - expired
            if (item.IsExpiringSoon)
                return new SolidColorBrush(Color.FromRgb(255, 185, 0));   // Yellow - expiring soon
            return new SolidColorBrush(Color.FromRgb(16, 137, 62));       // Green - OK
        }
        return new SolidColorBrush(Color.FromRgb(16, 137, 62));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Progress percentage to width converter (for progress bars)
/// </summary>
public class ProgressToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && 
            values[0] is double percentage && 
            values[1] is double totalWidth)
        {
            return totalWidth * (percentage / 100.0);
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
