using CulinaryAssistant.Domain.Interfaces;
using CulinaryAssistant.Infrastructure.Data;
using CulinaryAssistant.Infrastructure.Repositories;

namespace CulinaryAssistant.Infrastructure;

/// <summary>
/// Unit of Work - coordinates repositories and manages transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CulinaryDbContext _context;
    private IRecipeRepository? _recipes;
    private IInventoryRepository? _inventory;
    private IShoppingListRepository? _shoppingLists;
    private ICategoryRepository? _categories;

    public UnitOfWork(CulinaryDbContext context)
    {
        _context = context;
    }

    public IRecipeRepository Recipes => _recipes ??= new RecipeRepository(_context);
    public IInventoryRepository Inventory => _inventory ??= new InventoryRepository(_context);
    public IShoppingListRepository ShoppingLists => _shoppingLists ??= new ShoppingListRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
