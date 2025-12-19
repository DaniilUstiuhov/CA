using CulinaryAssistant.Domain.Entities;
using CulinaryAssistant.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CulinaryAssistant.Infrastructure.Data;

/// <summary>
/// Контекст базы данных
/// </summary>
public class CulinaryDbContext : DbContext
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RecipeCategory> RecipeCategories => Set<RecipeCategory>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();

    public CulinaryDbContext(DbContextOptions<CulinaryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Recipe configuration
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique(); // Уникальный бизнес-идентификатор
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Cuisine).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Instructions).HasMaxLength(10000);
            entity.Property(e => e.ImagePath).HasMaxLength(500);

            entity.HasMany(e => e.Ingredients)
                  .WithOne(i => i.Recipe)
                  .HasForeignKey(i => i.RecipeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.RecipeCategories)
                  .WithOne(rc => rc.Recipe)
                  .HasForeignKey(rc => rc.RecipeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // RecipeIngredient configuration
        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconName).HasMaxLength(50);
        });

        // RecipeCategory (N-N) configuration
        modelBuilder.Entity<RecipeCategory>(entity =>
        {
            entity.HasKey(rc => new { rc.RecipeId, rc.CategoryId });

            entity.HasOne(rc => rc.Recipe)
                  .WithMany(r => r.RecipeCategories)
                  .HasForeignKey(rc => rc.RecipeId);

            entity.HasOne(rc => rc.Category)
                  .WithMany(c => c.RecipeCategories)
                  .HasForeignKey(rc => rc.CategoryId);
        });

        // TPH (Table Per Hierarchy) для Item
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasDiscriminator<string>("ItemType")
                  .HasValue<InventoryItem>("Inventory")
                  .HasValue<ShoppingItem>("Shopping");
        });

        // InventoryItem configuration
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.Property(e => e.StorageLocation).HasMaxLength(100);
        });

        // ShoppingItem configuration
        modelBuilder.Entity<ShoppingItem>(entity =>
        {
            entity.Property(e => e.PreferredStore).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.EstimatedPrice).HasPrecision(10, 2);

            entity.HasOne(e => e.ShoppingList)
                  .WithMany(l => l.Items)
                  .HasForeignKey(e => e.ShoppingListId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ShoppingList configuration
        modelBuilder.Entity<ShoppingList>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new { Id = 1, Name = "Завтрак", Description = "Блюда для завтрака", IconName = "breakfast", CreatedAt = DateTime.UtcNow },
            new { Id = 2, Name = "Обед", Description = "Блюда для обеда", IconName = "lunch", CreatedAt = DateTime.UtcNow },
            new { Id = 3, Name = "Ужин", Description = "Блюда для ужина", IconName = "dinner", CreatedAt = DateTime.UtcNow },
            new { Id = 4, Name = "Праздничные", Description = "Праздничные блюда", IconName = "celebration", CreatedAt = DateTime.UtcNow },
            new { Id = 5, Name = "Быстрые", Description = "Быстрые в приготовлении", IconName = "fast", CreatedAt = DateTime.UtcNow }
        );

        // Seed Recipes
        modelBuilder.Entity<Recipe>().HasData(
            new
            {
                Id = 1,
                Code = "KHARCHO-001",
                Name = "Харчо",
                Cuisine = "Грузинская",
                DishType = DishType.FirstCourse,
                Status = RecipeStatus.Published,
                CookingTimeMinutes = 90,
                Servings = 6,
                Description = "Традиционный грузинский суп с говядиной, рисом и орехами.",
                Instructions = "1. Отварить говядину до готовности.\n2. Добавить рис и варить 15 минут.\n3. Добавить орехи и специи.\n4. Подавать горячим с зеленью.",
                CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            },
            new
            {
                Id = 2,
                Code = "KHACH-001",
                Name = "Хачапури по-аджарски",
                Cuisine = "Грузинская",
                DishType = DishType.MainCourse,
                Status = RecipeStatus.Published,
                CookingTimeMinutes = 45,
                Servings = 2,
                Description = "Грузинская лепешка с сыром и яйцом в форме лодочки.",
                Instructions = "1. Приготовить тесто.\n2. Сформировать лодочки.\n3. Добавить сыр.\n4. Запечь, добавить яйцо и масло.",
                CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            },
            new
            {
                Id = 3,
                Code = "CARB-001",
                Name = "Паста Карбонара",
                Cuisine = "Итальянская",
                DishType = DishType.MainCourse,
                Status = RecipeStatus.Draft,
                CookingTimeMinutes = 30,
                Servings = 4,
                Description = "Классическая итальянская паста с беконом и сливочным соусом.",
                Instructions = "1. Отварить пасту.\n2. Обжарить бекон.\n3. Смешать яйца с пармезаном.\n4. Соединить все ингредиенты.",
                CreatedAt = DateTime.UtcNow
            },
            new
            {
                Id = 4,
                Code = "BORSCH-001",
                Name = "Борщ украинский",
                Cuisine = "Украинская",
                DishType = DishType.FirstCourse,
                Status = RecipeStatus.Published,
                CookingTimeMinutes = 120,
                Servings = 8,
                Description = "Традиционный украинский суп со свеклой.",
                Instructions = "1. Сварить мясной бульон.\n2. Обжарить овощи.\n3. Добавить свеклу и капусту.\n4. Подавать со сметаной и пампушками.",
                CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            }
        );

        // Seed RecipeIngredients
        modelBuilder.Entity<RecipeIngredient>().HasData(
            // Харчо
            new { Id = 1, RecipeId = 1, Name = "Говядина", Amount = 500.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 2, RecipeId = 1, Name = "Рис", Amount = 100.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 3, RecipeId = 1, Name = "Грецкие орехи", Amount = 100.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 4, RecipeId = 1, Name = "Томатная паста", Amount = 2.0, Unit = MeasurementUnit.Tablespoon, IsOptional = false, CreatedAt = DateTime.UtcNow },
            // Хачапури
            new { Id = 5, RecipeId = 2, Name = "Мука", Amount = 500.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 6, RecipeId = 2, Name = "Сыр сулугуни", Amount = 300.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 7, RecipeId = 2, Name = "Яйца", Amount = 2.0, Unit = MeasurementUnit.Piece, IsOptional = false, CreatedAt = DateTime.UtcNow },
            // Карбонара
            new { Id = 8, RecipeId = 3, Name = "Спагетти", Amount = 400.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 9, RecipeId = 3, Name = "Бекон", Amount = 200.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 10, RecipeId = 3, Name = "Пармезан", Amount = 100.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            // Борщ
            new { Id = 11, RecipeId = 4, Name = "Свекла", Amount = 300.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 12, RecipeId = 4, Name = "Капуста", Amount = 300.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow },
            new { Id = 13, RecipeId = 4, Name = "Картофель", Amount = 400.0, Unit = MeasurementUnit.Gram, IsOptional = false, CreatedAt = DateTime.UtcNow }
        );

        // Seed RecipeCategories (N-N)
        modelBuilder.Entity<RecipeCategory>().HasData(
            new { RecipeId = 1, CategoryId = 2, AssignedAt = DateTime.UtcNow },
            new { RecipeId = 1, CategoryId = 4, AssignedAt = DateTime.UtcNow },
            new { RecipeId = 2, CategoryId = 1, AssignedAt = DateTime.UtcNow },
            new { RecipeId = 2, CategoryId = 5, AssignedAt = DateTime.UtcNow },
            new { RecipeId = 3, CategoryId = 3, AssignedAt = DateTime.UtcNow },
            new { RecipeId = 3, CategoryId = 5, AssignedAt = DateTime.UtcNow },
            new { RecipeId = 4, CategoryId = 2, AssignedAt = DateTime.UtcNow },
            new { RecipeId = 4, CategoryId = 4, AssignedAt = DateTime.UtcNow }
        );

        // Seed InventoryItems
        modelBuilder.Entity<InventoryItem>().HasData(
            new { Id = 101, Name = "Молоко", Quantity = 2.0, Unit = MeasurementUnit.Liter, ExpirationDate = DateTime.UtcNow.AddDays(5), StorageLocation = "Холодильник", CreatedAt = DateTime.UtcNow },
            new { Id = 102, Name = "Яйца", Quantity = 10.0, Unit = MeasurementUnit.Piece, ExpirationDate = DateTime.UtcNow.AddDays(14), StorageLocation = "Холодильник", CreatedAt = DateTime.UtcNow },
            new { Id = 103, Name = "Мука", Quantity = 1.0, Unit = MeasurementUnit.Kilogram, ExpirationDate = DateTime.UtcNow.AddDays(180), StorageLocation = "Шкаф", CreatedAt = DateTime.UtcNow },
            new { Id = 104, Name = "Сыр", Quantity = 300.0, Unit = MeasurementUnit.Gram, ExpirationDate = DateTime.UtcNow.AddDays(2), StorageLocation = "Холодильник", CreatedAt = DateTime.UtcNow }
        );

        // Seed ShoppingLists
        modelBuilder.Entity<ShoppingList>().HasData(
            new { Id = 1, Name = "Продукты на неделю", Description = "Основные продукты", IsCompleted = false, CreatedAt = DateTime.UtcNow },
            new { Id = 2, Name = "Для праздника", Description = "Ингредиенты для праздничного стола", IsCompleted = false, CreatedAt = DateTime.UtcNow }
        );

        // Seed ShoppingItems
        modelBuilder.Entity<ShoppingItem>().HasData(
            new { Id = 201, ShoppingListId = 1, Name = "Хлеб", Quantity = 2.0, Unit = MeasurementUnit.Piece, IsPurchased = false, PreferredStore = "Rimi", CreatedAt = DateTime.UtcNow },
            new { Id = 202, ShoppingListId = 1, Name = "Масло сливочное", Quantity = 200.0, Unit = MeasurementUnit.Gram, IsPurchased = true, PurchasedAt = DateTime.UtcNow, EstimatedPrice = 2.99m, PreferredStore = "Prisma", CreatedAt = DateTime.UtcNow },
            new { Id = 203, ShoppingListId = 2, Name = "Красная икра", Quantity = 100.0, Unit = MeasurementUnit.Gram, IsPurchased = false, EstimatedPrice = 15.99m, PreferredStore = "Stockmann", CreatedAt = DateTime.UtcNow }
        );
    }
}
