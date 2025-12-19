using CulinaryAssistant.Domain.Interfaces;
using CulinaryAssistant.Infrastructure.Data;
using CulinaryAssistant.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

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
    private IDbContextTransaction? _transaction;

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

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
