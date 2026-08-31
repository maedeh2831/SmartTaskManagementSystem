using Microsoft.Extensions.Logging;
using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Gamification;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

/// <summary>
/// اقلام فعال باید اثر واقعی داشته باشند: حاشیه آواتار، نشان، پوسته و ضریب تجربه
/// </summary>
public class EquippedCosmeticsServiceTests
{
    private static EquippedCosmeticsService CreateService(ApplicationDbContext context)
        => new(context, new Mock<ILogger<EquippedCosmeticsService>>().Object);

    private static MarketplaceItem AddItem(
        ApplicationDbContext context,
        string name,
        string category,
        string color = "#123456",
        string icon = "🔵",
        MarketplaceItemRarity rarity = MarketplaceItemRarity.Common)
    {
        var item = new MarketplaceItem
        {
            Name = name,
            Description = "توضیح",
            Icon = icon,
            Color = color,
            Category = category,
            Rarity = rarity,
            Price = 100,
            Stock = -1,
            IsActive = true
        };
        context.Set<MarketplaceItem>().Add(item);
        context.SaveChanges();
        return item;
    }

    private static void Equip(ApplicationDbContext context, int userId, int itemId, bool equipped = true)
    {
        context.Set<UserInventory>().Add(new UserInventory
        {
            UserId = userId,
            MarketplaceItemId = itemId,
            Quantity = 1,
            IsEquipped = equipped,
            AcquiredDate = DateTime.UtcNow,
            ViewState = true
        });
        context.SaveChanges();
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsEquippedAvatarBorder()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var item = AddItem(context, "Golden Ring Border", "Avatar Border",
            color: "#FFD700", icon: "💛", rarity: MarketplaceItemRarity.Uncommon);
        Equip(context, userId, item.Id);

        var result = await CreateService(context).GetForUserAsync(userId);

        Assert.Equal("#FFD700", result.AvatarBorderColor);
        Assert.Equal("Golden Ring Border", result.AvatarBorderName);
        Assert.Equal(2, result.AvatarBorderRarity);
        Assert.True(result.HasAny);
    }

    [Fact]
    public async Task GetForUserAsync_IgnoresUnequippedItems()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var item = AddItem(context, "Flame Border", "Avatar Border");
        Equip(context, userId, item.Id, equipped: false);

        var result = await CreateService(context).GetForUserAsync(userId);

        Assert.Null(result.AvatarBorderColor);
        Assert.False(result.HasAny);
    }

    [Fact]
    public async Task GetForUserAsync_DoesNotLeakOtherUsersItems()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var item = AddItem(context, "Purple Glow Border", "Avatar Border");
        Equip(context, userId, item.Id);

        var result = await CreateService(context).GetForUserAsync(userId + 999);

        Assert.Null(result.AvatarBorderColor);
    }

    [Fact]
    public async Task GetForUserAsync_ConvertsThemeNameToCssSlug()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var item = AddItem(context, "Ocean Blue Theme", "Theme", color: "#1E90FF");
        Equip(context, userId, item.Id);

        var result = await CreateService(context).GetForUserAsync(userId);

        // این کلید در CSS به صورت [data-shop-theme="ocean-blue"] استفاده می‌شود
        Assert.Equal("ocean-blue", result.ThemeSlug);
        Assert.Equal("Ocean Blue Theme", result.ThemeName);
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsBadgeAndPerksTogether()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var badge = AddItem(context, "Team Player Badge", "Badge", icon: "🤝");
        var perk = AddItem(context, "Double XP Boost", "Perk");
        Equip(context, userId, badge.Id);
        Equip(context, userId, perk.Id);

        var result = await CreateService(context).GetForUserAsync(userId);

        Assert.Equal("🤝", result.BadgeIcon);
        Assert.Contains("Double XP Boost", result.ActivePerks);
    }

    [Fact]
    public async Task GetExperienceMultiplierAsync_DefaultsToOne_WhenNoPerksEquipped()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;

        var multiplier = await CreateService(context).GetExperienceMultiplierAsync(userId);

        Assert.Equal(1.0, multiplier);
    }

    [Fact]
    public async Task GetExperienceMultiplierAsync_DoublesForDoubleXpBoost()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var perk = AddItem(context, "Double XP Boost", "Perk");
        Equip(context, userId, perk.Id);

        var multiplier = await CreateService(context).GetExperienceMultiplierAsync(userId);

        Assert.Equal(2.0, multiplier);
    }

    [Fact]
    public async Task GetExperienceMultiplierAsync_StacksMultiplePerks()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        Equip(context, userId, AddItem(context, "Double XP Boost", "Perk").Id);
        Equip(context, userId, AddItem(context, "VIP Access", "Perk").Id);

        var multiplier = await CreateService(context).GetExperienceMultiplierAsync(userId);

        Assert.Equal(3.0, multiplier); // 2.0 × 1.5
    }

    [Fact]
    public async Task HasActivePerkAsync_ReflectsEquippedState()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var perk = AddItem(context, "Priority Support", "Perk");
        Equip(context, userId, perk.Id);
        var service = CreateService(context);

        Assert.True(await service.HasActivePerkAsync(userId, "Priority Support"));
        Assert.False(await service.HasActivePerkAsync(userId, "VIP Access"));
    }
}
