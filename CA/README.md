# Culinary Assistant (WPF, Clean Architecture)

Desktop app to manage recipes, pantry inventory, and shopping lists. Built on .NET 8, WPF, and MVVM with a Clean Architecture layout (Domain -> Application -> Infrastructure -> UI).

## Features
- Recipe management with Draft -> Published -> Archived workflow, ingredients, instructions, images, and cuisine/dish typing.
- Inventory tracking with quantities, units, expiration handling, and storage locations.
- Shopping lists with completion tracking, purchased counts, and per-item metadata (notes, store, estimated price).
- Categories (N-N to recipes) with unique names and icon hints.
- Dashboard with aggregates (recipes, categories, expiring items, active shopping lists).
- CSV export for recipes, recipe details, inventory, and shopping lists (UTF-8 with BOM; ';' delimiter).
- Validation via FluentValidation; logging via Serilog (daily rolling files).

## Architecture
- **Domain (`CulinaryAssistant.Domain`)**: Entities (Recipe, RecipeIngredient, Category, InventoryItem/ShoppingItem via TPH, ShoppingList), value enums, domain services, and exceptions. Business rules live here (status transitions, uniqueness, required fields).
- **Application (`CulinaryAssistant.Application`)**: DTOs, validators, and services (recipes, inventory, shopping lists, categories, dashboard, CSV export). Uses repositories/Unit of Work abstractions.
- **Infrastructure (`CulinaryAssistant.Infrastructure`)**: EF Core 8 + SQLite context, configurations, repositories, and UnitOfWork. Migration/seed setup in `Data/CulinaryDbContext`.
- **UI (`CulinaryAssistant.UI`)**: WPF MVVM (CommunityToolkit.Mvvm), DI host, views (Dashboard, Recipe list/detail, Inventory, Shopping lists), converters, and navigation/dialog services. Startup wiring and DB migration live in `CulinaryAssistant.UI/App.xaml.cs`.

## Tech Stack & Requirements
- .NET 8 SDK
- Windows (WPF desktop)
- SQLite (file DB)
- Optional: `dotnet-ef` global tool for migrations (`dotnet tool install --global dotnet-ef`)
- Recommended: Visual Studio 2022 or VS Code with C# Dev Kit

## Getting Started
1) Restore: `dotnet restore CulinaryAssistant.sln`
2) Build (optional but recommended): `dotnet build CulinaryAssistant.sln`
3) Run migrations (automatic): `CulinaryAssistant.UI/App.xaml.cs` calls `Database.Migrate()` on startup, creating `culinary.db` and applying seeds.
   - Manual apply (if needed):  
     `dotnet ef database update --project CulinaryAssistant.Infrastructure --startup-project CulinaryAssistant.UI`
4) Launch the app: `dotnet run --project CulinaryAssistant.UI`
   - Logs go to `CulinaryAssistant.UI/logs/culinary-<date>.txt`.
   - Delete `culinary.db` to recreate with seed data.

### Adding a migration
```bash
dotnet ef migrations add <Name> \
  --project CulinaryAssistant.Infrastructure \
  --startup-project CulinaryAssistant.UI
```

## Seed Data (first run)
- 5 categories
- 4 recipes (3 Published, 1 Draft) with linked ingredients and categories
- 4 inventory items (with expiration dates)
- 2 shopping lists with 3 items total (progress tracking)
The seed lives in `CulinaryAssistant.Infrastructure/Data/CulinaryDbContext.cs`.

## Domain Rules (high level)
- Recipe `Code` required, uppercased, <=20 chars; unique index enforced. Name and cuisine required; cooking time and servings must be >0.
- Status workflow: Draft -> Publish (requires instructions + >=1 ingredient) -> Archive -> Restore or return to Draft.
- Category names unique (indexed).
- Relationships: Recipe 1-N Ingredients; Recipe N-N Categories via `RecipeCategory`.
- Items use TPH inheritance (`Item` -> `InventoryItem`/`ShoppingItem`) with per-type fields.
- Shopping lists track purchased/total counts and completion percentage.

## Project Layout
```
CulinaryAssistant/
+- CulinaryAssistant.sln
+- CulinaryAssistant.Domain/               # Entities, enums, interfaces, domain exceptions
+- CulinaryAssistant.Application/          # DTOs, validators, services, contracts
+- CulinaryAssistant.Infrastructure/       # EF Core context, repositories, UnitOfWork, migrations/seed
L- CulinaryAssistant.UI/                   # WPF app, views/viewmodels, DI bootstrap, logging
```

## Exports
- Recipe list, recipe detail, inventory, shopping list exports return UTF-8 BOM CSV with ';' delimiter (see `CulinaryAssistant.Application/Services/OtherServices.cs` for formats).

## Logging
- Serilog writes daily rolling files to `CulinaryAssistant.UI/logs/`. Update the sink in `CulinaryAssistant.UI/App.xaml.cs` if you change paths.

## License
MIT
