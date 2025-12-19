using System.Windows;
using CulinaryAssistant.Application.DTOs;
using CulinaryAssistant.Application.Interfaces;
using CulinaryAssistant.Application.Services;
using CulinaryAssistant.Application.Validators;
using CulinaryAssistant.Domain.Interfaces;
using CulinaryAssistant.Infrastructure;
using CulinaryAssistant.Infrastructure.Data;
using CulinaryAssistant.UI.Services;
using CulinaryAssistant.UI.ViewModels;
using CulinaryAssistant.UI.Views;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CulinaryAssistant.UI;

/// <summary>
/// WPF Application - точка входа с настройкой DI
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    public static ServiceProvider ServiceProvider { get; private set; } = null!;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/culinary-.txt", 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        _serviceProvider = services.BuildServiceProvider();
        ServiceProvider = _serviceProvider;

        // Initialize database
        InitializeDatabase();

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // Database
        services.AddDbContext<CulinaryDbContext>(options =>
            options.UseSqlite("Data Source=culinary.db"));

        // Repositories (via UnitOfWork)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Validators
        services.AddScoped<IValidator<RecipeCreateDto>, RecipeCreateDtoValidator>();
        services.AddScoped<IValidator<RecipeUpdateDto>, RecipeUpdateDtoValidator>();
        services.AddScoped<IValidator<RecipeIngredientCreateDto>, RecipeIngredientCreateDtoValidator>();
        services.AddScoped<IValidator<InventoryItemCreateDto>, InventoryItemCreateDtoValidator>();
        services.AddScoped<IValidator<InventoryItemUpdateDto>, InventoryItemUpdateDtoValidator>();
        services.AddScoped<IValidator<ShoppingListCreateDto>, ShoppingListCreateDtoValidator>();
        services.AddScoped<IValidator<ShoppingItemCreateDto>, ShoppingItemCreateDtoValidator>();
        services.AddScoped<IValidator<CategoryCreateDto>, CategoryCreateDtoValidator>();

        // Application Services
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IShoppingListService, ShoppingListService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IExportService, ExportService>();

        // UI Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<RecipeListViewModel>();
        services.AddTransient<RecipeDetailViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<ShoppingListViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }

    private void InitializeDatabase()
    {
        using var scope = _serviceProvider!.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CulinaryDbContext>();
        context.Database.Migrate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
