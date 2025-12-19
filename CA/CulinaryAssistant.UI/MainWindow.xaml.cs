using System.Windows;
using CulinaryAssistant.UI.ViewModels;

namespace CulinaryAssistant.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
