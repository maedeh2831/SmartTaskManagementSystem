/*
| Module      : Controllers
| Controller  : GamificationController
| Purpose     : مدیریت API های گیمیفیکیشن
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Gamification;
using SmartTask.Web.Services.Gamification;

namespace SmartTask.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GamificationController : ControllerBase
    {
        private readonly IAchievementEngine _achievementEngine;
        private readonly IMilestoneService _milestoneService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GamificationController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMarketplaceService _marketplaceService;
        private readonly IPurchaseService _purchaseService;
        private readonly IEconomyAnalysisService _economyService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly IProductivityMetricsService _productivityMetricsService;
        private readonly IEquippedCosmeticsService _cosmeticsService;

        public GamificationController(
            IAchievementEngine achievementEngine,
            IMilestoneService milestoneService,
            ICurrentUserService currentUserService,
            ILogger<GamificationController> logger,
            ApplicationDbContext context,
            IMarketplaceService marketplaceService,
            IPurchaseService purchaseService,
            IEconomyAnalysisService economyService,
            ILeaderboardService leaderboardService,
            IProductivityMetricsService productivityMetricsService,
            IEquippedCosmeticsService cosmeticsService)
        {
            _achievementEngine = achievementEngine;
            _milestoneService = milestoneService;
            _currentUserService = currentUserService;
            _logger = logger;
            _context = context;
            _marketplaceService = marketplaceService;
            _purchaseService = purchaseService;
            _economyService = economyService;
            _leaderboardService = leaderboardService;
            _productivityMetricsService = productivityMetricsService;
            _cosmeticsService = cosmeticsService;
        }

        /// <summary>
        /// دریافت تمام دستاوردهای موجود
        /// </summary>
        [HttpGet("achievements")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllAchievements()
        {
            try
            {
                var achievements = await _context.Set<Models.Entities.Achievement>()
                    .Where(a => a.IsActive)
                    .Select(a => new AchievementDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        Icon = a.Icon,
                        Color = a.Color,
                        Rarity = (int)a.Rarity,
                        Category = (int)a.Category,
                        RewardPoints = a.RewardPoints,
                        RewardExperience = a.RewardExperience,
                        Condition = a.Condition,
                        ConditionValue = a.ConditionValue
                    })
                    .ToListAsync();

                // برای کاربر وارد‌شده، وضعیت باز شدن و پیشرفت هر دستاورد اضافه می‌شود
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _currentUserService.UserId;

                    var unlocked = await _context.Set<Models.Entities.UserAchievement>()
                        .Where(ua => ua.UserId == userId)
                        .Select(ua => new { ua.AchievementId, ua.UnlockedDate })
                        .ToListAsync();

                    var unlockedMap = unlocked
                        .GroupBy(x => x.AchievementId)
                        .ToDictionary(g => g.Key, g => g.First().UnlockedDate);

                    var progression = await _context.Set<Models.Entities.UserProgression>()
                        .FirstOrDefaultAsync(p => p.UserId == userId);

                    foreach (var dto in achievements)
                    {
                        if (unlockedMap.TryGetValue(dto.Id, out var unlockedDate))
                        {
                            dto.IsUnlocked = true;
                            dto.UnlockedDate = unlockedDate;
                            dto.CurrentProgress = dto.ConditionValue;
                            dto.ProgressPercent = 100;
                            continue;
                        }

                        dto.CurrentProgress = dto.Condition switch
                        {
                            "TasksCompleted" => progression?.TasksCompleted ?? 0,
                            "ProjectsCompleted" => progression?.ProjectsCompleted ?? 0,
                            "SprintsCompleted" => progression?.SprintsCompleted ?? 0,
                            _ => 0
                        };

                        dto.ProgressPercent = dto.ConditionValue > 0
                            ? Math.Min(100, (int)Math.Round(dto.CurrentProgress * 100.0 / dto.ConditionValue))
                            : 0;
                    }
                }

                return Ok(achievements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all achievements");
                return StatusCode(500, new { message = "Error retrieving achievements" });
            }
        }

        /// <summary>
        /// دریافت دستاوردهای آنلاک‌شده کاربر
        /// </summary>
        [HttpGet("achievements/{userId}")]
        public async Task<IActionResult> GetUserAchievements(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var userAchievements = await _context.Set<Models.Entities.UserAchievement>()
                    .Where(ua => ua.UserId == userId)
                    .Include(ua => ua.Achievement)
                    .Select(ua => new UserAchievementDto
                    {
                        AchievementId = ua.Achievement.Id,
                        Name = ua.Achievement.Name,
                        Description = ua.Achievement.Description,
                        Icon = ua.Achievement.Icon,
                        Color = ua.Achievement.Color,
                        Rarity = (int)ua.Achievement.Rarity,
                        Category = (int)ua.Achievement.Category,
                        RewardPoints = ua.Achievement.RewardPoints,
                        RewardExperience = ua.Achievement.RewardExperience,
                        UnlockedDate = ua.UnlockedDate
                    })
                    .OrderByDescending(ua => ua.UnlockedDate)
                    .ToListAsync();

                return Ok(userAchievements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievements for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving user achievements" });
            }
        }

        /// <summary>
        /// دریافت پیشرفت نقاط عطف کاربر
        /// </summary>
        [HttpGet("milestones/{userId}")]
        public async Task<IActionResult> GetUserMilestoneProgress(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var milestoneProgress = await _milestoneService.GetUserMilestoneProgressAsync(userId);

                var result = new List<MilestoneProgressDto>();
                foreach (var progress in milestoneProgress)
                {
                    var percentage = await _milestoneService.GetUserMilestoneCompletionPercentageAsync(userId, progress.MilestoneId);
                    result.Add(new MilestoneProgressDto
                    {
                        MilestoneId = progress.MilestoneId,
                        Name = progress.Milestone.Name,
                        Description = progress.Milestone.Description,
                        Icon = progress.Milestone.Icon,
                        Color = progress.Milestone.Color,
                        Type = (int)progress.Milestone.Type,
                        CurrentProgress = progress.CurrentProgress,
                        TargetValue = progress.TargetValue,
                        CompletionPercentage = percentage,
                        IsCompleted = progress.IsCompleted,
                        CompletedDate = progress.CompletedDate,
                        RewardPoints = progress.Milestone.RewardPoints,
                        RewardExperience = progress.Milestone.RewardExperience
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting milestones for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving user milestones" });
            }
        }

        /// <summary>
        /// دریافت تمام نقاط عطف موجود
        /// </summary>
        [HttpGet("milestones")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMilestones()
        {
            try
            {
                var milestones = await _milestoneService.GetAllMilestonesAsync();
                var result = milestones.Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Description,
                    m.Icon,
                    m.Color,
                    Type = (int)m.Type,
                    m.TargetValue,
                    m.RewardPoints,
                    m.RewardExperience,
                    m.DisplayOrder
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all milestones");
                return StatusCode(500, new { message = "Error retrieving milestones" });
            }
        }

        #region Marketplace Endpoints

        /// <summary>
        /// دریافت اقلام بازار با فیلتر‌های اختیاری
        /// GET /api/gamification/marketplace/items?category=Badge&skip=0&take=20
        /// </summary>
        [HttpGet("marketplace/items")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMarketplaceItems(string? category = null, int skip = 0, int take = 20)
        {
            try
            {
                var items = await _marketplaceService.GetAllItemsAsync(category, skip, take);

                // If user is authenticated, add ownership info
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = _currentUserService.UserId;
                    foreach (var item in items)
                    {
                        var hasItem = await _purchaseService.HasItemAsync(userId, item.Id);
                        item.IsOwned = hasItem;

                        if (hasItem)
                        {
                            var inventoryItem = await _purchaseService.GetInventoryItemAsync(userId, item.Id);
                            if (inventoryItem != null)
                            {
                                item.OwnedQuantity = inventoryItem.Quantity;
                                item.IsEquipped = inventoryItem.IsEquipped;
                            }
                        }
                    }
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace items");
                return StatusCode(500, new { message = "Error retrieving marketplace items" });
            }
        }

        /// <summary>
        /// دریافت دسته‌بندی‌های موجود
        /// GET /api/gamification/marketplace/categories
        /// </summary>
        [HttpGet("marketplace/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMarketplaceCategories()
        {
            try
            {
                var categories = await _marketplaceService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace categories");
                return StatusCode(500, new { message = "Error retrieving categories" });
            }
        }

        /// <summary>
        /// خریداری اقلام
        /// POST /api/gamification/marketplace/purchase/{itemId}
        /// </summary>
        [HttpPost("marketplace/purchase/{itemId}")]
        public async Task<IActionResult> PurchaseItem(int itemId, [FromBody] PurchaseRequestViewModel request)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var quantity = request?.Quantity ?? 1;

                if (quantity < 1)
                    return BadRequest(new { message = "Quantity must be at least 1" });

                var (success, message, remainingPoints) = await _purchaseService.PurchaseItemAsync(userId, itemId, quantity);

                if (!success)
                    return BadRequest(new { message, remainingPoints });

                var inventoryItem = await _purchaseService.GetInventoryItemAsync(userId, itemId);

                return Ok(new PurchaseResponseViewModel
                {
                    Success = true,
                    Message = message,
                    RemainingPoints = remainingPoints,
                    InventoryItem = inventoryItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing item {ItemId}", itemId);
                return StatusCode(500, new { message = "Error processing purchase" });
            }
        }

        /// <summary>
        /// دریافت موجودی کاربر
        /// GET /api/gamification/inventory/{userId}
        /// </summary>
        [HttpGet("inventory/{userId}")]
        public async Task<IActionResult> GetUserInventory(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var inventory = await _purchaseService.GetUserInventoryAsync(userId);
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving inventory" });
            }
        }

        /// <summary>
        /// تجهیز یک اقلام
        /// POST /api/gamification/inventory/{inventoryId}/equip
        /// </summary>
        [HttpPost("inventory/{inventoryId}/equip")]
        public async Task<IActionResult> EquipItem(int inventoryId)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var inventory = await _context.Set<Models.Entities.UserInventory>()
                    .FirstOrDefaultAsync(x => x.Id == inventoryId);

                if (inventory == null || inventory.UserId != userId)
                    return NotFound(new { message = "Inventory item not found" });

                var success = await _purchaseService.EquipItemAsync(userId, inventoryId);
                if (!success)
                    return BadRequest(new { message = "Failed to equip item" });

                // اقلام فعال برگردانده می‌شود تا رابط کاربری بدون رفرش اعمال کند
                var cosmetics = await _cosmeticsService.GetForUserAsync(userId);
                return Ok(new { message = "Item equipped successfully", cosmetics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error equipping item");
                return StatusCode(500, new { message = "Error equipping item" });
            }
        }

        /// <summary>
        /// خلع یک اقلام
        /// POST /api/gamification/inventory/{inventoryId}/unequip
        /// </summary>
        [HttpPost("inventory/{inventoryId}/unequip")]
        public async Task<IActionResult> UnequipItem(int inventoryId)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var inventory = await _context.Set<Models.Entities.UserInventory>()
                    .FirstOrDefaultAsync(x => x.Id == inventoryId);

                if (inventory == null || inventory.UserId != userId)
                    return NotFound(new { message = "Inventory item not found" });

                var success = await _purchaseService.UnequipItemAsync(userId, inventoryId);
                if (!success)
                    return BadRequest(new { message = "Failed to unequip item" });

                var cosmetics = await _cosmeticsService.GetForUserAsync(userId);
                return Ok(new { message = "Item unequipped successfully", cosmetics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unequipping item");
                return StatusCode(500, new { message = "Error unequipping item" });
            }
        }

        /// <summary>
        /// دریافت متریکس اقتصادی بازار (فقط Admin)
        /// GET /api/gamification/economy/metrics
        /// </summary>
        [HttpGet("economy/metrics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEconomyMetrics()
        {
            try
            {
                var metrics = await _economyService.GetMarketplaceMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving economy metrics");
                return StatusCode(500, new { message = "Error retrieving metrics" });
            }
        }

        /// <summary>
        /// دریافت بیشترین فروش‌ها (فقط Admin)
        /// GET /api/gamification/economy/top-items?take=10
        /// </summary>
        [HttpGet("economy/top-items")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTopSellingItems(int take = 10)
        {
            try
            {
                var topItems = await _economyService.GetTopSellingItemsAsync(take);
                return Ok(topItems.Select(x => new
                {
                    ItemName = x.ItemName,
                    TotalSold = x.TotalSold,
                    Revenue = x.Revenue
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top selling items");
                return StatusCode(500, new { message = "Error retrieving top items" });
            }
        }

        #endregion

        #region Leaderboards

        /// <summary>
        /// دریافت رتبه‌بندی جهانی
        /// </summary>
        [HttpGet("leaderboards/global")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGlobalLeaderboard(int page = 1, int pageSize = 50, string timeRange = "all")
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var (entries, totalCount) = await _leaderboardService.GetGlobalLeaderboardAsync(page, pageSize, timeRange);

                return Ok(new
                {
                    data = entries,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (totalCount + pageSize - 1) / pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving global leaderboard");
                return StatusCode(500, new { message = "Error retrieving leaderboard" });
            }
        }

        /// <summary>
        /// دریافت رتبه‌بندی Workspace
        /// </summary>
        [HttpGet("leaderboards/workspace/{workspaceId}")]
        public async Task<IActionResult> GetWorkspaceLeaderboard(int workspaceId, int page = 1, int pageSize = 50, string timeRange = "all")
        {
            try
            {
                // Check if user is member of workspace
                var isMember = await _context.Set<Models.Entities.WorkspaceMember>()
                    .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.ApplicationUserId == _currentUserService.UserId);

                if (!isMember && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var (entries, totalCount) = await _leaderboardService.GetWorkspaceLeaderboardAsync(workspaceId, page, pageSize, timeRange);

                return Ok(new
                {
                    data = entries,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (totalCount + pageSize - 1) / pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workspace leaderboard for workspace {WorkspaceId}", workspaceId);
                return StatusCode(500, new { message = "Error retrieving workspace leaderboard" });
            }
        }

        /// <summary>
        /// دریافت رتبه‌بندی تیم‌های Workspace
        /// </summary>
        [HttpGet("leaderboards/teams/{workspaceId}")]
        public async Task<IActionResult> GetTeamLeaderboard(int workspaceId, string timeRange = "all")
        {
            try
            {
                // Check if user is member of workspace
                var isMember = await _context.Set<Models.Entities.WorkspaceMember>()
                    .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.ApplicationUserId == _currentUserService.UserId);

                if (!isMember && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var (entries, totalCount) = await _leaderboardService.GetTeamLeaderboardAsync(workspaceId, timeRange);

                return Ok(new
                {
                    data = entries,
                    pagination = new
                    {
                        totalCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team leaderboard for workspace {WorkspaceId}", workspaceId);
                return StatusCode(500, new { message = "Error retrieving team leaderboard" });
            }
        }

        /// <summary>
        /// دریافت رتبه کاربر و همسایگان
        /// </summary>
        [HttpGet("leaderboards/user/{userId}")]
        public async Task<IActionResult> GetUserLeaderboardContext(int userId, string timeRange = "all")
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var context = await _leaderboardService.GetUserLeaderboardContextAsync(userId, timeRange);

                return Ok(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user leaderboard context for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving user leaderboard context" });
            }
        }

        /// <summary>
        /// دریافت رتبه تیم کاربر
        /// </summary>
        [HttpGet("leaderboards/user/{userId}/team-rank")]
        public async Task<IActionResult> GetUserTeamRank(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var (teamRank, totalTeams) = await _leaderboardService.GetUserTeamRankAsync(userId);

                return Ok(new
                {
                    teamRank,
                    totalTeams
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user team rank for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving team rank" });
            }
        }

        /// <summary>
        /// دریافت ورودی رتبه‌بندی کاربر
        /// </summary>
        [HttpGet("leaderboards/user-entry/{userId}")]
        public async Task<IActionResult> GetUserLeaderboardEntry(int userId, int? workspaceId = null)
        {
            try
            {
                var entry = await _leaderboardService.GetUserLeaderboardEntryAsync(userId, workspaceId);

                if (entry == null)
                {
                    return NotFound(new { message = "User not found in leaderboard" });
                }

                return Ok(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user leaderboard entry for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving user leaderboard entry" });
            }
        }

        #endregion

        #region User Profile with Productivity

        /// <summary>
        /// دریافت پروفایل کاربر شامل متریکس بهره‌وری
        /// GET /api/gamification/profile/{userId}
        /// </summary>
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var user = await _context.Set<Models.Entities.ApplicationUser>()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                var achievements = await _context.Set<Models.Entities.UserAchievement>()
                    .Where(ua => ua.UserId == userId)
                    .Include(ua => ua.Achievement)
                    .CountAsync();

                var milestones = await _context.Set<Models.Entities.UserMilestoneProgress>()
                    .Where(ump => ump.UserId == userId && ump.IsCompleted)
                    .CountAsync();

                var productivity = await _productivityMetricsService.GetUserProductivityAsync(userId);
                var benchmarks = await _productivityMetricsService.GetBenchmarkMetricsAsync(userId);

                var wallet = await _context.Set<Models.Entities.UserWallet>()
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                return Ok(new
                {
                    user = new { user.Id, user.FirstName, user.LastName, user.Email, user.Avatar },
                    achievements,
                    milestonesCompleted = milestones,
                    productivity,
                    benchmarks,
                    wallet = new { points = wallet?.AvailablePoints ?? 0, premium = 0 }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving user profile" });
            }
        }

        #endregion
    }
}