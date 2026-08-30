# Phase 3: Marketplace & Economy System - SmartTask Momentum

## Overview

Phase 3 implements a complete marketplace and economy system for SmartTask Momentum gamification. Users can earn points through task completion and spend them on cosmetic items and perks to customize their experience.

## Architecture

### Entities Created

1. **MarketplaceItem** - Represents purchasable items in the marketplace
   - Properties: Name, Description, Icon, Color, Category, Rarity, Price, Stock
   - Supports limited-time items with availability windows
   - Tracks total sales for analytics

2. **UserInventory** - Tracks items owned by users
   - Properties: UserId, MarketplaceItemId, Quantity, IsEquipped
   - Supports equipping/unequipping items
   - Unique constraint: user can only have one inventory entry per item

3. **MarketplaceTransaction** - Records all purchase transactions
   - Properties: UserId, UserWalletId, MarketplaceItemId, PointsSpent, Quantity, Status
   - Stores transaction history for auditing
   - Tracks transaction status (Pending, Completed, Failed, Refunded)

### Services

#### IMarketplaceService / MarketplaceService
- **GetAllItemsAsync()** - Retrieve items with category filtering
- **GetItemByIdAsync()** - Get specific item details
- **GetCategoriesAsync()** - Get all available categories
- **CreateItemAsync()** - Admin: Create new marketplace items
- **UpdateItemAsync()** - Admin: Update existing items
- **DeleteItemAsync()** - Admin: Soft delete items
- **UpdateStockAsync()** - Admin: Manage stock levels
- **IsItemAvailableAsync()** - Check if item is available for purchase
- **GetAvailableStockAsync()** - Get remaining stock

#### IPurchaseService / PurchaseService
- **PurchaseItemAsync()** - Purchase item with double-spend prevention via locking
- **GetUserInventoryAsync()** - Retrieve all items owned by user
- **GetInventoryItemAsync()** - Get specific inventory item
- **EquipItemAsync()** - Equip item for display
- **UnequipItemAsync()** - Remove item from display
- **HasItemAsync()** - Check if user owns item

**Security Features:**
- Thread-safe locking mechanism per user to prevent race conditions
- Database transactions for atomic operations
- Wallet balance validation before deduction
- Stock validation for limited items

#### IEconomyAnalysisService / EconomyAnalysisService
- **GetMarketplaceMetricsAsync()** - Overall marketplace statistics
- **GetUserEconomyStatsAsync()** - Per-user economy data
- **GetTopSellingItemsAsync()** - Revenue analytics
- **GetCategoryDistributionAsync()** - Sales distribution by category

### API Endpoints

#### Public Endpoints (Authenticated)
```
GET    /api/gamification/marketplace/items?category=&skip=0&take=20
GET    /api/gamification/marketplace/categories
GET    /api/gamification/inventory/{userId}
POST   /api/gamification/marketplace/purchase/{itemId}
POST   /api/gamification/inventory/{inventoryId}/equip
POST   /api/gamification/inventory/{inventoryId}/unequip
```

#### Admin Endpoints
```
GET    /api/gamification/economy/metrics
GET    /api/gamification/economy/top-items?take=10
```

### Marketplace Categories

1. **Avatar Border** - Visual customization for user profiles
2. **Badge** - Achievement and milestone badges
3. **Theme** - UI theme customizations
4. **Perk** - Special abilities and bonuses

### Rarity System

- **Common** (1) - Basic items, low cost
- **Uncommon** (2) - Standard items, moderate cost
- **Rare** (3) - Special items, higher cost
- **Epic** (4) - Powerful items, expensive
- **Legendary** (5) - Ultra-rare items, very expensive

### Seeded Items (37 total)

#### Avatar Borders (6 items)
- Simple Blue Border (Common) - 100 points
- Green Circle Border (Common) - 100 points
- Golden Ring Border (Uncommon) - 250 points
- Purple Glow Border (Uncommon) - 250 points
- Diamond Sparkle Border (Rare) - 500 points
- Flame Border (Rare) - 500 points

#### Badges (6 items)
- First Task Badge (Common) - 50 points
- Quick Starter Badge (Common) - 75 points
- 100 Tasks Master (Uncommon) - 200 points
- Team Player Badge (Uncommon) - 200 points
- Legendary Finisher (Rare) - 400 points
- Perfect Score Badge (Rare) - 400 points

#### Themes (6 items)
- Light Theme (Common) - FREE
- Dark Theme (Common) - FREE
- Ocean Blue Theme (Uncommon) - 150 points
- Forest Green Theme (Uncommon) - 150 points
- Neon Cyberpunk Theme (Rare) - 300 points
- Sunset Orange Theme (Rare) - 300 points

#### Perks (7 items)
- Double XP Boost (Uncommon) - 200 points
- Priority Support (Uncommon) - 250 points
- Triple Points Weekend (Rare, Limited) - 400 points
- Team Synchronizer (Rare) - 350 points
- VIP Access (Epic) - 750 points
- Legendary Crown Border (Legendary, Limited) - 1000 points

### Database Migration

Migration: `20260829_Phase3_Marketplace.cs`

Creates three main tables:
- `MarketplaceItems` - Marketplace item catalog
- `UserInventories` - User item ownership
- `MarketplaceTransactions` - Purchase history

Includes optimized indexes for:
- Category filtering
- User inventory lookups
- Transaction queries by date/user
- Limited-time item availability

### Views

#### Marketplace.cshtml
- Browse items by category
- View current wallet balance
- Purchase items with quantity selection
- Real-time purchase confirmation
- Owned item indicators

#### Inventory.cshtml
- View all owned items organized by category
- Equip/Unequip items
- Track acquisition date
- View item quantities
- Filter by category

### Double-Spend Prevention

The PurchaseService implements multi-level protection:

```csharp
// User-level locking
static Dictionary<int, object> _userLocks = new();
lock (userLock) {
    // Database transaction
    using (var transaction = _context.Database.BeginTransaction()) {
        // Balance validation
        // Stock validation
        // Point deduction
        // Inventory update
        // Transaction recording
        transaction.Commit();
    }
}
```

## Integration Points

### Dependency Injection (Program.cs)
```csharp
builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IEconomyAnalysisService, EconomyAnalysisService>();
```

### Controller Integration
GamificationController extended with marketplace endpoints and economy monitoring.

### Seeding (Program.cs)
```csharp
await MarketplaceItemSeeder.SeedMarketplaceItemsAsync(context);
```

## Usage Examples

### Purchase Item
```javascript
POST /api/gamification/marketplace/purchase/5
{
  "quantity": 1
}

Response:
{
  "success": true,
  "message": "Purchase successful",
  "remainingPoints": 4900,
  "inventoryItem": { ... }
}
```

### Get User Inventory
```javascript
GET /api/gamification/inventory/1

Response: [
  {
    "id": 1,
    "itemId": 5,
    "itemName": "Diamond Sparkle Border",
    "quantity": 1,
    "isEquipped": true,
    "acquiredDate": "2026-08-29T10:30:00Z"
  }
]
```

### Equip Item
```javascript
POST /api/gamification/inventory/1/equip

Response: { "message": "Item equipped successfully" }
```

### Economy Metrics (Admin)
```javascript
GET /api/gamification/economy/metrics

Response:
{
  "totalItems": 37,
  "totalSales": 145,
  "totalRevenue": 45600,
  "uniqueBuyers": 23,
  "averageTransactionValue": 314.48,
  "timestamp": "2026-08-29T15:45:00Z"
}
```

## Security Considerations

1. **Double-Spend Prevention** - Thread-safe locking + database transactions
2. **Balance Validation** - Always check wallet before deduction
3. **Stock Management** - Prevent overselling limited items
4. **Authorization** - User can only access own inventory
5. **Admin Endpoints** - Restricted to Admin role
6. **Audit Trail** - All transactions recorded with user/timestamp

## Files Created

### Entities
- `Models/Entities/MarketplaceItem.cs`
- `Models/Entities/UserInventory.cs`
- `Models/Entities/MarketplaceTransaction.cs`

### Services
- `Services/Gamification/IMarketplaceService.cs`
- `Services/Gamification/MarketplaceService.cs`
- `Services/Gamification/IPurchaseService.cs`
- `Services/Gamification/PurchaseService.cs`
- `Services/Gamification/IEconomyAnalysisService.cs`
- `Services/Gamification/EconomyAnalysisService.cs`

### DTOs
- `Models/ViewModels/Gamification/MarketplaceItemDto.cs`
- `Models/ViewModels/Gamification/UserInventoryDto.cs`
- `Models/ViewModels/Gamification/PurchaseRequestViewModel.cs`

### Configuration & Seeding
- `Data/Configurations/MarketplaceItemConfiguration.cs`
- `Data/Configurations/UserInventoryConfiguration.cs`
- `Data/Configurations/MarketplaceTransactionConfiguration.cs`
- `Infrastructure/Seed/MarketplaceItemSeeder.cs`

### Enums
- `Models/Enums/MarketplaceItemRarity.cs`

### Views
- `Views/Gamification/Marketplace.cshtml`
- `Views/Gamification/Inventory.cshtml`

### Migration
- `Migrations/20260829_Phase3_Marketplace.cs`

### Updated Files
- `Data/Context/ApplicationDbContext.cs` - Added DbSets
- `Controllers/GamificationController.cs` - Added endpoints
- `Program.cs` - Registered services and seeder

## Next Steps

1. Apply database migration: `dotnet ef database update`
2. Test marketplace endpoints via Swagger/Postman
3. Monitor economy metrics for balance
4. Implement additional item types as needed
5. Add item rarity visual indicators to UI
6. Consider limited-time promotions system

## Performance Optimizations

- Indexed queries for category/user lookups
- Paged API responses (default take=20)
- Materialized inventory views for fast retrieval
- Analytics queries optimized with GroupBy/Select

## Future Enhancements

- Trading between users
- Auction system for rare items
- Seasonal marketplace rotations
- Item crafting/combination
- Marketplace events and flash sales
- Leaderboards based on spending/collection
