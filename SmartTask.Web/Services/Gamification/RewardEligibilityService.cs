/*
| Module      : Gamification
| Class       : RewardEligibilityService
| Purpose     : دروازه ضد‌سوء‌استفاده پیش از اعطای پاداش
|
| قواعد اعمال‌شده:
|   ۱. پاداش هر تسک فقط یک‌بار (بر اساس دفتر تراکنش‌ها) — جلوی چرخه Done→ToDo→Done را می‌گیرد
|   ۲. تسک باید واقعاً در وضعیت Done باشد
|   ۳. کاربر باید به تسک تخصیص داشته باشد
|   ۴. تسک‌های بی‌محتوا (بدون عنوان معنادار) پاداش نمی‌گیرند
|   ۵. سقف تعداد پاداش در ساعت و در روز
|   ۶. تسک‌های فوری‌ساخته‌شده (ایجاد و تکمیل در چند ثانیه) پاداش نمی‌گیرند
|   ۷. کاربران تعلیق‌شده پاداش نمی‌گیرند
*/

using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Gamification
{
    public class RewardEligibilityService : IRewardEligibilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAbuseDetectionEngine _abuseEngine;
        private readonly ILogger<RewardEligibilityService> _logger;

        // سقف‌های محافظه‌کارانه؛ کاربر واقعی به این حدها نمی‌رسد
        private const int MaxRewardsPerHour = 20;
        private const int MaxRewardsPerDay = 60;

        // تسکی که در فاصله کمتر از این مقدار ساخته و تکمیل شود مشکوک است
        private static readonly TimeSpan MinimumTaskLifetime = TimeSpan.FromSeconds(30);

        private const int MinimumTitleLength = 3;

        public RewardEligibilityService(
            ApplicationDbContext context,
            IAbuseDetectionEngine abuseEngine,
            ILogger<RewardEligibilityService> logger)
        {
            _context = context;
            _abuseEngine = abuseEngine;
            _logger = logger;
        }

        public async Task<bool> HasAlreadyRewardedTaskAsync(int userId, int taskId)
        {
            // دفتر تراکنش‌ها منبع حقیقت است: اگر قبلاً برای این تسک امتیاز داده شده،
            // تغییر دوباره وضعیت پاداش جدید ایجاد نمی‌کند.
            return await _context.Set<WalletTransaction>()
                .AnyAsync(t => t.RelatedTaskId == taskId
                               && t.TransactionType == TransactionType.Earned
                               && t.UserWallet.UserId == userId);
        }

        public async Task<RewardEligibility> CanRewardTaskAsync(int userId, int taskId)
        {
            try
            {
                if (userId <= 0 || taskId <= 0)
                    return RewardEligibility.Deny("شناسه نامعتبر");

                // ۱. پاداش تکراری برای همان تسک
                if (await HasAlreadyRewardedTaskAsync(userId, taskId))
                    return RewardEligibility.Deny("برای این تسک قبلاً پاداش داده شده است");

                // ۷. کاربر تعلیق‌شده
                if (await _abuseEngine.IsUserSuspendedAsync(userId))
                    return RewardEligibility.Deny("پاداش‌های این کاربر موقتاً تعلیق شده است");

                var task = await _context.TaskItems
                    .Include(t => t.Assignments.Where(a => a.ViewState))
                    .FirstOrDefaultAsync(t => t.Id == taskId);

                if (task == null)
                    return RewardEligibility.Deny("تسک یافت نشد");

                // ۲. باید واقعاً تکمیل شده باشد
                if (task.Status != TaskStatusType.Done)
                    return RewardEligibility.Deny("تسک در وضعیت تکمیل‌شده نیست");

                // ۳. کاربر باید به تسک تخصیص داشته باشد
                var isAssigned = task.Assignments.Any(a => a.ApplicationUserId == userId);
                if (!isAssigned)
                    return RewardEligibility.Deny("کاربر به این تسک تخصیص ندارد");

                // ۴. تسک بی‌محتوا (مثل «a» یا «...»)
                var title = (task.Title ?? string.Empty).Trim();
                if (title.Length < MinimumTitleLength)
                    return RewardEligibility.Deny("عنوان تسک برای دریافت پاداش معتبر نیست");

                // ۶. تسک فوری‌ساخته‌شده برای گرفتن امتیاز
                var completedAt = task.CompletedDate ?? DateTime.Now;
                var lifetime = completedAt - task.CreatedDate;
                if (lifetime < MinimumTaskLifetime)
                {
                    _logger.LogWarning(
                        "Reward denied: task {TaskId} completed {Seconds}s after creation by user {UserId}",
                        taskId, lifetime.TotalSeconds, userId);
                    return RewardEligibility.Deny("تسک بلافاصله پس از ایجاد تکمیل شده است");
                }

                // ۵. سقف نرخ پاداش
                var now = DateTime.UtcNow;

                var rewardsLastHour = await CountRecentRewardsAsync(userId, now.AddHours(-1));
                if (rewardsLastHour >= MaxRewardsPerHour)
                {
                    _logger.LogWarning(
                        "Reward denied: user {UserId} hit hourly cap ({Count})", userId, rewardsLastHour);
                    return RewardEligibility.Deny("سقف پاداش ساعتی پر شده است");
                }

                var rewardsLastDay = await CountRecentRewardsAsync(userId, now.AddDays(-1));
                if (rewardsLastDay >= MaxRewardsPerDay)
                {
                    _logger.LogWarning(
                        "Reward denied: user {UserId} hit daily cap ({Count})", userId, rewardsLastDay);
                    return RewardEligibility.Deny("سقف پاداش روزانه پر شده است");
                }

                return RewardEligibility.Allow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking reward eligibility for user {UserId}, task {TaskId}", userId, taskId);

                // در صورت خطا محافظه‌کارانه عمل می‌کنیم تا امتیاز اشتباه داده نشود
                return RewardEligibility.Deny("بررسی شرایط پاداش ناموفق بود");
            }
        }

        private async Task<int> CountRecentRewardsAsync(int userId, DateTime since)
        {
            return await _context.Set<WalletTransaction>()
                .CountAsync(t => t.UserWallet.UserId == userId
                                 && t.TransactionType == TransactionType.Earned
                                 && t.RelatedTaskId != null
                                 && t.TransactionDate >= since);
        }
    }
}
