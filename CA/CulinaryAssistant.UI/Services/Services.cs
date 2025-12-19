using System.Windows;
using Microsoft.Win32;

namespace CulinaryAssistant.UI.Services;

/// <summary>
/// Navigation service interface
/// </summary>
public interface INavigationService
{
    event Action<string>? NavigationRequested;
    void NavigateTo(string viewName, object? parameter = null);
    object? CurrentParameter { get; }
}

/// <summary>
/// Navigation service implementation
/// </summary>
public class NavigationService : INavigationService
{
    public event Action<string>? NavigationRequested;
    public object? CurrentParameter { get; private set; }

    public void NavigateTo(string viewName, object? parameter = null)
    {
        CurrentParameter = parameter;
        NavigationRequested?.Invoke(viewName);
    }
}

/// <summary>
/// Dialog service interface
/// </summary>
public interface IDialogService
{
    void ShowInfo(string message, string title = "Информация");
    void ShowWarning(string message, string title = "Предупреждение");
    void ShowError(string message, string title = "Ошибка");
    bool ShowConfirm(string message, string title = "Подтверждение");
    string? ShowSaveFileDialog(string filter, string defaultFileName = "");
    string? ShowOpenFileDialog(string filter);
}

/// <summary>
/// Dialog service implementation
/// </summary>
public class DialogService : IDialogService
{
    public void ShowInfo(string message, string title = "Информация")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowWarning(string message, string title = "Предупреждение")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public void ShowError(string message, string title = "Ошибка")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool ShowConfirm(string message, string title = "Подтверждение")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName = "")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName
        };
        
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowOpenFileDialog(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter
        };
        
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
