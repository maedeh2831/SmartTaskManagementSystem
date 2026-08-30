/*
| Module      : Gamification
| Class       : EquippedCosmeticsService
| Purpose     : خواندن اقلام فعال کاربر و تبدیل آن‌ها به مقادیر قابل استفاده در ظاهر
*/

using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Gamification;

namespace SmartTask.Web.Services.Gamification
{
    public class EquippedCosmeticsService : IEquippedCosmeticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquippedCosmeticsService> _logger;

        public EquippedCosmeticsService(
            ApplicationDbContext context,
            ILogger<EquippedCosmeticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EquippedCosmeticsDto> GetForUserAsync(int userId)
        {
            var result = new EquippedCosmeticsDto();

            if (userId <= 0)
                return result;

            try
            {
                var equipped = await _context.Set<UserInventory>()
                    .Where(x => x.UserId == userId && x.IsEquipped && x.ViewState)
                    .Include(x => x.MarketplaceItem)
                    .Select(x => x.MarketplaceItem)
                    .Where(x => x != null)
                    .ToListAsync();

                foreach (var item in equipped)
                {
                    if (item == null) continue;

                    switch (item.Category)
                    {
                        case "Avatar Border":
                            result.AvatarBorderColor = item.Color;
                            result.AvatarBorderIcon = item.Icon;
                            result.AvatarBorderName = item.Name;
                            result.AvatarBorderRarity = (int)item.Rarity;
                            break;

                        case "Badge":
                            result.BadgeIcon = item.Icon;
                            result.BadgeName = item.Name;
                            result.BadgeColor = item.Color;
                            break;

                        case "Theme":
                            result.ThemeName = item.Name;
                            result.ThemeColor = item.Color;
                            result.ThemeSlug = ToSlug(item.Name);
                            break;

                        case "Perk":
                            result.ActivePerks.Add(item.Name);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading equipped cosmetics for user {UserId}", userId);
            }

            return result;
        }

        public async Task<bool> HasActivePerkAsync(int userId, string perkName)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(perkName))
                return false;

            try
            {
                return await _context.Set<UserInventory>()
                    .Include(x => x.MarketplaceItem)
                    .AnyAsync(x => x.UserId == userId
                                   && x.IsEquipped
                                   && x.ViewState
                                   && x.MarketplaceItem != null
                                   && x.MarketplaceItem.Category == "Perk"
                                   && x.MarketplaceItem.Name == perkName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking perk {PerkName} for user {UserId}", perkName, userId);
                return false;
            }
        }

        public async Task<double> GetExperienceMultiplierAsync(int userId)
        {
            var multiplier = 1.0;

            if (userId <= 0)
                return multiplier;

            try
            {
                var perks = await _context.Set<UserInventory>()
                    .Include(x => x.MarketplaceItem)
                    .Where(x => x.UserId == userId
                                && x.IsEquipped
                                && x.ViewState
                                && x.MarketplaceItem != null
                                && x.MarketplaceItem.Category == "Perk")
                    .Select(x => x.MarketplaceItem!.Name)
                    .ToListAsync();

                // مزایای فعال ضریب تجربه را افزایش می‌دهند
                if (perks.Contains("Double XP Boost"))
                    multiplier *= 2.0;

                if (perks.Contains("Triple Points Weekend"))
                    multiplier *= 3.0;

                if (perks.Contains("VIP Access"))
                    multiplier *= 1.5;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing experience multiplier for user {UserId}", userId);
            }

            return multiplier;
        }

        /// <summary>
        /// تبدیل نام قلم به کلید CSS (مثل "Ocean Blue Theme" → "ocean-blue")
        /// </summary>
        private static string ToSlug(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var cleaned = name.Replace("Theme", "", StringComparison.OrdinalIgnoreCase).Trim();

            return cleaned
                .ToLowerInvariant()
                .Replace(' ', '-');
        }
    }
}
