using Microsoft.Extensions.Logging;
using Moq;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Gamification;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

/// <summary>
/// پوشش مسیر واقعی خرید → انبار → فعال‌سازی
/// </summary>
public class PurchaseServiceInventoryTests
{
    private static PurchaseService CreateService(SmartTask.Web.Data.Context.ApplicationDbContext context)
    {
        return new PurchaseService(context, new Mock<ILogger<PurchaseService>>().Object);
    }

    private static (int userId, int itemId) SeedWalletAndItem(
        SmartTask.Web.Data.Context.ApplicationDbContext context,
        int points = 1000,
        int price = 100)
    {
        var user = context.Set<ApplicationUser>().First();

        context.Set<UserWallet>().Add(new UserWallet
        {
            UserId = user.Id,
            TotalPoints = points,
            AvailablePoints = points,
            LastUpdated = DateTime.UtcNow
        });

        var item = new MarketplaceItem
        {
            Name = "Test Border",
            Description = "توضیح آزمایشی",
            Icon = "🔵",
            Color = "#0066FF",
            Category = "Avatar Border",
            Rarity = MarketplaceItemRarity.Common,
            Price = price,
            Stock = -1,
            IsActive = true
        };
        context.Set<MarketplaceItem>().Add(item);
        context.SaveChanges();

        return (user.Id, item.Id);
    }

    [Fact]
    public async Task GetUserInventoryAsync_ReturnsItemNameIconAndDescription()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, itemId) = SeedWalletAndItem(context);
        var service = CreateService(context);

        var (success, _, _) = await service.PurchaseItemAsync(userId, itemId, 1);
        Assert.True(success);

        var inventory = await service.GetUserInventoryAsync(userId);

        var dto = Assert.Single(inventory);
        // این فیلدها همان‌هایی هستند که صفحه انبار نمایش می‌دهد
        Assert.Equal("Test Border", dto.ItemName);
        Assert.Equal("🔵", dto.ItemIcon);
        Assert.Equal("Avatar Border", dto.Category);
        Assert.Equal("توضیح آزمایشی", dto.ItemDescription);
    }

    [Fact]
    public async Task EquipItemAsync_MarksInventoryRowEquipped()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, itemId) = SeedWalletAndItem(context);
        var service = CreateService(context);

        await service.PurchaseItemAsync(userId, itemId, 1);
        var inventory = await service.GetUserInventoryAsync(userId);
        var inventoryId = inventory.Single().Id;

        var equipped = await service.EquipItemAsync(userId, inventoryId);

        Assert.True(equipped);
        var refreshed = await service.GetUserInventoryAsync(userId);
        Assert.True(refreshed.Single().IsEquipped);
    }

    [Fact]
    public async Task EquipItemAsync_ReturnsFalse_ForOtherUsersInventory()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, itemId) = SeedWalletAndItem(context);
        var service = CreateService(context);

        await service.PurchaseItemAsync(userId, itemId, 1);
        var inventoryId = (await service.GetUserInventoryAsync(userId)).Single().Id;

        // کاربر دیگری نباید بتواند قلم این کاربر را فعال کند
        var equipped = await service.EquipItemAsync(userId + 999, inventoryId);

        Assert.False(equipped);
    }

    [Fact]
    public async Task EquipItemAsync_UnequipsOtherItemsInSameCategory()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, firstItemId) = SeedWalletAndItem(context, points: 1000, price: 100);

        var second = new MarketplaceItem
        {
            Name = "Second Border",
            Description = "دومی",
            Icon = "🟢",
            Color = "#00CC00",
            Category = "Avatar Border",
            Rarity = MarketplaceItemRarity.Common,
            Price = 100,
            Stock = -1,
            IsActive = true
        };
        context.Set<MarketplaceItem>().Add(second);
        context.SaveChanges();

        var service = CreateService(context);
        await service.PurchaseItemAsync(userId, firstItemId, 1);
        await service.PurchaseItemAsync(userId, second.Id, 1);

        var inventory = await service.GetUserInventoryAsync(userId);
        var firstInv = inventory.First(x => x.ItemId == firstItemId);
        var secondInv = inventory.First(x => x.ItemId == second.Id);

        await service.EquipItemAsync(userId, firstInv.Id);
        await service.EquipItemAsync(userId, secondInv.Id);

        var refreshed = await service.GetUserInventoryAsync(userId);
        // فقط یک قلم از هر دسته باید فعال باشد
        Assert.False(refreshed.First(x => x.ItemId == firstItemId).IsEquipped);
        Assert.True(refreshed.First(x => x.ItemId == second.Id).IsEquipped);
    }

    [Fact]
    public async Task UnequipItemAsync_ClearsEquippedFlag()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, itemId) = SeedWalletAndItem(context);
        var service = CreateService(context);

        await service.PurchaseItemAsync(userId, itemId, 1);
        var inventoryId = (await service.GetUserInventoryAsync(userId)).Single().Id;
        await service.EquipItemAsync(userId, inventoryId);

        var result = await service.UnequipItemAsync(userId, inventoryId);

        Assert.True(result);
        Assert.False((await service.GetUserInventoryAsync(userId)).Single().IsEquipped);
    }
}
