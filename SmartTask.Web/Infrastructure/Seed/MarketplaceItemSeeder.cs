/*
| Module      : Infrastructure
| Class       : MarketplaceItemSeeder
| Purpose     : بذر کردن داده‌های اولیه بازار
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Infrastructure.Seed
{
    public static class MarketplaceItemSeeder
    {
        public static async Task SeedMarketplaceItemsAsync(ApplicationDbContext context)
        {
            try
            {
                // Check if items already exist
                if (context.Set<MarketplaceItem>().Any())
                {
                    Console.WriteLine("Marketplace items already seeded.");
                    await SeedAdditionalItemsAsync(context);
                    return;
                }

                var items = new List<MarketplaceItem>
                {
                    // Avatar Borders - Common
                    new MarketplaceItem
                    {
                        Name = "Simple Blue Border",
                        Description = "A clean blue border for your avatar",
                        Icon = "🔵",
                        Color = "#0066FF",
                        Category = "Avatar Border",
                        Rarity = MarketplaceItemRarity.Common,
                        Price = 100,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 1,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Green Circle Border",
                        Description = "A fresh green border for your profile",
                        Icon = "🟢",
                        Color = "#00CC00",
                        Category = "Avatar Border",
                        Rarity = MarketplaceItemRarity.Common,
                        Price = 100,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 2,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Avatar Borders - Uncommon
                    new MarketplaceItem
                    {
                        Name = "Golden Ring Border",
                        Description = "Elegant gold border with a professional look",
                        Icon = "💛",
                        Color = "#FFD700",
                        Category = "Avatar Border",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 250,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 3,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Purple Glow Border",
                        Description = "Mystical purple border with glow effect",
                        Icon = "💜",
                        Color = "#9933FF",
                        Category = "Avatar Border",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 250,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 4,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Avatar Borders - Rare
                    new MarketplaceItem
                    {
                        Name = "Diamond Sparkle Border",
                        Description = "Rare diamond-studded border with sparkle effect",
                        Icon = "💎",
                        Color = "#00FFFF",
                        Category = "Avatar Border",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 500,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 5,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Flame Border",
                        Description = "Animated fire border - shows you're on fire!",
                        Icon = "🔥",
                        Color = "#FF6600",
                        Category = "Avatar Border",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 500,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 6,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Badges - Common
                    new MarketplaceItem
                    {
                        Name = "First Task Badge",
                        Description = "Celebrate your first completed task",
                        Icon = "🏅",
                        Color = "#FFB700",
                        Category = "Badge",
                        Rarity = MarketplaceItemRarity.Common,
                        Price = 50,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 10,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Quick Starter Badge",
                        Description = "Complete 5 tasks quickly",
                        Icon = "⚡",
                        Color = "#FFFF00",
                        Category = "Badge",
                        Rarity = MarketplaceItemRarity.Common,
                        Price = 75,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 11,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Badges - Uncommon
                    new MarketplaceItem
                    {
                        Name = "100 Tasks Master",
                        Description = "Earned after completing 100 tasks",
                        Icon = "🎖️",
                        Color = "#FF9900",
                        Category = "Badge",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 200,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 12,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Team Player Badge",
                        Description = "Collaborate with team members",
                        Icon = "🤝",
                        Color = "#00AA00",
                        Category = "Badge",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 200,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 13,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Badges - Rare
                    new MarketplaceItem
                    {
                        Name = "Legendary Finisher",
                        Description = "Complete all tasks in a sprint without missing",
                        Icon = "👑",
                        Color = "#FF00FF",
                        Category = "Badge",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 400,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 14,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Perfect Score Badge",
                        Description = "Achieve perfect task execution",
                        Icon = "⭐",
                        Color = "#FFD700",
                        Category = "Badge",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 400,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 15,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Themes - Common
                    new MarketplaceItem
                    {
                        Name = "Light Theme",
                        Description = "Bright and clean light interface",
                        Icon = "☀️",
                        Color = "#FFFFFF",
                        Category = "Theme",
                        Rarity = MarketplaceItemRarity.Common,
                        Price = 0,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 20,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Dark Theme",
                        Description = "Easy on the eyes dark interface",
                        Icon = "🌙",
                        Color = "#1A1A1A",
                        Category = "Theme",
                        Rarity = MarketplaceItemRarity.Common,
                        Price = 0,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 21,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Themes - Uncommon
                    new MarketplaceItem
                    {
                        Name = "Ocean Blue Theme",
                        Description = "Calm ocean-inspired color scheme",
                        Icon = "🌊",
                        Color = "#1E90FF",
                        Category = "Theme",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 150,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 22,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Forest Green Theme",
                        Description = "Natural and refreshing green palette",
                        Icon = "🌲",
                        Color = "#228B22",
                        Category = "Theme",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 150,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 23,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Themes - Rare
                    new MarketplaceItem
                    {
                        Name = "Neon Cyberpunk Theme",
                        Description = "Futuristic neon-colored interface",
                        Icon = "💻",
                        Color = "#FF00FF",
                        Category = "Theme",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 300,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 24,
                        CreatedDate = DateTime.UtcNow
                    },
                    new MarketplaceItem
                    {
                        Name = "Sunset Orange Theme",
                        Description = "Warm sunset-inspired interface",
                        Icon = "🌅",
                        Color = "#FF7F50",
                        Category = "Theme",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 300,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 25,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Perks - Uncommon
                    new MarketplaceItem
                    {
                        Name = "Double XP Boost",
                        Description = "Earn 2x experience points for 7 days",
                        Icon = "⚙️",
                        Color = "#FF5500",
                        Category = "Perk",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 200,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 30,
                        CreatedDate = DateTime.UtcNow,
                        IsLimitedTime = false
                    },
                    new MarketplaceItem
                    {
                        Name = "Priority Support",
                        Description = "Get priority response on support tickets",
                        Icon = "🎫",
                        Color = "#0099FF",
                        Category = "Perk",
                        Rarity = MarketplaceItemRarity.Uncommon,
                        Price = 250,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 31,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Perks - Rare
                    new MarketplaceItem
                    {
                        Name = "Triple Points Weekend",
                        Description = "Get 3x points on all tasks this weekend",
                        Icon = "📈",
                        Color = "#00FF00",
                        Category = "Perk",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 400,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 32,
                        CreatedDate = DateTime.UtcNow,
                        IsLimitedTime = true,
                        AvailableFrom = DateTime.UtcNow,
                        AvailableUntil = DateTime.UtcNow.AddDays(7)
                    },
                    new MarketplaceItem
                    {
                        Name = "Team Synchronizer",
                        Description = "Sync tasks with 10 additional team members",
                        Icon = "🔗",
                        Color = "#9900FF",
                        Category = "Perk",
                        Rarity = MarketplaceItemRarity.Rare,
                        Price = 350,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 33,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Perks - Epic
                    new MarketplaceItem
                    {
                        Name = "VIP Access",
                        Description = "Access to all premium features for 30 days",
                        Icon = "✨",
                        Color = "#FFD700",
                        Category = "Perk",
                        Rarity = MarketplaceItemRarity.Epic,
                        Price = 750,
                        Stock = -1,
                        IsActive = true,
                        DisplayOrder = 34,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Limited Time Item - Legendary
                    new MarketplaceItem
                    {
                        Name = "Legendary Crown Border",
                        Description = "Ultra-rare crown border - limited time only!",
                        Icon = "👑",
                        Color = "#FFD700",
                        Category = "Avatar Border",
                        Rarity = MarketplaceItemRarity.Legendary,
                        Price = 1000,
                        Stock = 100,
                        IsActive = true,
                        DisplayOrder = 100,
                        CreatedDate = DateTime.UtcNow,
                        IsLimitedTime = true,
                        AvailableFrom = DateTime.UtcNow,
                        AvailableUntil = DateTime.UtcNow.AddDays(30)
                    }
                };

                context.Set<MarketplaceItem>().AddRange(items);
                await context.SaveChangesAsync();

                Console.WriteLine($"Successfully seeded {items.Count} marketplace items.");

                await SeedAdditionalItemsAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding marketplace items: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// افزودن اقلام تکمیلی به بازار (idempotent — بر اساس نام بررسی می‌شود)
        /// </summary>
        private static async Task SeedAdditionalItemsAsync(ApplicationDbContext context)
        {
            try
            {
                var existingNames = context.Set<MarketplaceItem>()
                    .Select(x => x.Name)
                    .ToHashSet();

                var extras = BuildAdditionalItems()
                    .Where(x => !existingNames.Contains(x.Name))
                    .ToList();

                if (extras.Count == 0)
                    return;

                context.Set<MarketplaceItem>().AddRange(extras);
                await context.SaveChangesAsync();

                Console.WriteLine($"Successfully seeded {extras.Count} additional marketplace items.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding additional marketplace items: {ex.Message}");
            }
        }

        private static List<MarketplaceItem> BuildAdditionalItems() => new()
        {
            // ── Avatar Borders ──
            new MarketplaceItem
            {
                Name = "Silver Frost Border",
                Description = "حاشیه نقره‌ای با درخشش سرد و حرفه‌ای",
                Icon = "🩶",
                Color = "#B0C4DE",
                Category = "Avatar Border",
                Rarity = MarketplaceItemRarity.Common,
                Price = 120,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 7,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Rose Gold Border",
                Description = "حاشیه رزگلد شیک برای پروفایل شما",
                Icon = "🌸",
                Color = "#E8A0A0",
                Category = "Avatar Border",
                Rarity = MarketplaceItemRarity.Uncommon,
                Price = 280,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 8,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Galaxy Nebula Border",
                Description = "حاشیه کهکشانی با افکت ستاره‌های متحرک",
                Icon = "🌌",
                Color = "#6A0DAD",
                Category = "Avatar Border",
                Rarity = MarketplaceItemRarity.Epic,
                Price = 700,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 9,
                CreatedDate = DateTime.UtcNow
            },

            // ── Badges ──
            new MarketplaceItem
            {
                Name = "Early Bird Badge",
                Description = "برای کسانی که کارها را پیش از موعد تمام می‌کنند",
                Icon = "🌅",
                Color = "#FFA726",
                Category = "Badge",
                Rarity = MarketplaceItemRarity.Common,
                Price = 90,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 16,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Night Owl Badge",
                Description = "نشان کسانی که شب‌ها بهترین عملکرد را دارند",
                Icon = "🦉",
                Color = "#5C6BC0",
                Category = "Badge",
                Rarity = MarketplaceItemRarity.Common,
                Price = 90,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 17,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Bug Hunter Badge",
                Description = "برای شکارچیان خطا و بهبوددهندگان کیفیت",
                Icon = "🐞",
                Color = "#E53935",
                Category = "Badge",
                Rarity = MarketplaceItemRarity.Uncommon,
                Price = 220,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 18,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Sprint Champion Badge",
                Description = "قهرمان اسپرینت — بالاترین امتیاز در یک اسپرینت",
                Icon = "🏆",
                Color = "#FFB300",
                Category = "Badge",
                Rarity = MarketplaceItemRarity.Epic,
                Price = 650,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 19,
                CreatedDate = DateTime.UtcNow
            },

            // ── Themes ──
            new MarketplaceItem
            {
                Name = "Midnight Purple Theme",
                Description = "تم بنفش تیره برای تمرکز بیشتر در شب",
                Icon = "🌃",
                Color = "#4A148C",
                Category = "Theme",
                Rarity = MarketplaceItemRarity.Uncommon,
                Price = 170,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 26,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Sakura Blossom Theme",
                Description = "تم الهام‌گرفته از شکوفه‌های گیلاس",
                Icon = "🌷",
                Color = "#F48FB1",
                Category = "Theme",
                Rarity = MarketplaceItemRarity.Rare,
                Price = 320,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 27,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Monochrome Pro Theme",
                Description = "تم تک‌رنگ مینیمال برای حرفه‌ای‌ها",
                Icon = "◼️",
                Color = "#37474F",
                Category = "Theme",
                Rarity = MarketplaceItemRarity.Rare,
                Price = 320,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 28,
                CreatedDate = DateTime.UtcNow
            },

            // ── Perks ──
            new MarketplaceItem
            {
                Name = "Streak Shield",
                Description = "یک روز غیبت، رشته فعالیت شما را نمی‌شکند",
                Icon = "🛡️",
                Color = "#00897B",
                Category = "Perk",
                Rarity = MarketplaceItemRarity.Uncommon,
                Price = 260,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 35,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Custom Profile Banner",
                Description = "بنر اختصاصی برای صفحه پروفایل شما",
                Icon = "🖼️",
                Color = "#8E24AA",
                Category = "Perk",
                Rarity = MarketplaceItemRarity.Rare,
                Price = 380,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 36,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Extra Task Slots",
                Description = "افزودن ۱۰ اسلات تسک اضافه به داشبورد شخصی",
                Icon = "➕",
                Color = "#43A047",
                Category = "Perk",
                Rarity = MarketplaceItemRarity.Uncommon,
                Price = 230,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 37,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Advanced Analytics Pack",
                Description = "دسترسی به نمودارها و گزارش‌های پیشرفته بهره‌وری",
                Icon = "📊",
                Color = "#1E88E5",
                Category = "Perk",
                Rarity = MarketplaceItemRarity.Epic,
                Price = 800,
                Stock = -1,
                IsActive = true,
                DisplayOrder = 38,
                CreatedDate = DateTime.UtcNow
            },
            new MarketplaceItem
            {
                Name = "Golden Trophy Showcase",
                Description = "ویترین افسانه‌ای برای نمایش دستاوردهای شما",
                Icon = "🏅",
                Color = "#FFD700",
                Category = "Perk",
                Rarity = MarketplaceItemRarity.Legendary,
                Price = 1200,
                Stock = 50,
                IsActive = true,
                DisplayOrder = 101,
                CreatedDate = DateTime.UtcNow,
                IsLimitedTime = true,
                AvailableFrom = DateTime.UtcNow,
                AvailableUntil = DateTime.UtcNow.AddDays(45)
            }
        };
    }
}
