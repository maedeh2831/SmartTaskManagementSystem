/*
| Module      : Gamification Services
| Interface   : IProductivityMetricsService
| Purpose     : تعریف رابط سرویس متریکس بهره‌وری
*/

using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Gamification;

namespace SmartTask.Web.Services.Gamification
{
    public interface IProductivityMetricsService
    {
        /// <summary>
        /// محاسبهٔ نمرهٔ بهره‌وری کاربر (0-100)
        /// </summary>
        Task<double> CalculateProductivityScoreAsync(int userId, string period = "month");

        /// <summary>
        /// محاسبهٔ درصد تکمیل کارها
        /// </summary>
        Task<double> CalculateTaskCompletionRateAsync(int userId, string period = "month");

        /// <summary>
        /// محاسبهٔ درصد تحویل به‌موقع
        /// </summary>
        Task<double> CalculateOnTimeDeliveryRateAsync(int userId, string period = "month");

        /// <summary>
        /// محاسبهٔ نمرهٔ کیفیت (کارهای بدون بازگشایی)
        /// </summary>
        Task<double> CalculateQualityScoreAsync(int userId, string period = "month");

        /// <summary>
        /// محاسبهٔ درصد سازگاری (روزهای کار کردهٔ / کل روزهای دوره)
        /// </summary>
        Task<double> CalculateConsistencyRateAsync(int userId, string period = "month");

        /// <summary>
        /// به‌روزرسانی رشتهٔ فعالیت کاربر
        /// </summary>
        Task<int> UpdateStreakAsync(int userId);

        /// <summary>
        /// دریافت سطح بهره‌وری کاربر
        /// </summary>
        Task<ProductivityTier> GetProductivityTierAsync(int userId);

        /// <summary>
        /// دریافت متریکس بهره‌وری کاربر
        /// </summary>
        Task<ProductivityMetricsDto> GetUserProductivityAsync(int userId);

        /// <summary>
        /// دریافت متریکس بهره‌وری تیم (جمع‌آوری‌شده)
        /// </summary>
        Task<TeamProductivityDto> GetTeamProductivityAsync(int teamId);

        /// <summary>
        /// به‌روزرسانی متریکس بهره‌وری کاربر
        /// </summary>
        Task<bool> UpdateProductivityMetricsAsync(int userId, int workspaceId);

        /// <summary>
        /// دریافت تاریخچهٔ نمرات
        /// </summary>
        Task<List<ProductivityScoreDto>> GetScoreHistoryAsync(int userId, int days = 30);

        /// <summary>
        /// دریافت معیارهای تطابق (میانگین کاربر vs تیم vs workspace)
        /// </summary>
        Task<BenchmarkMetricsDto> GetBenchmarkMetricsAsync(int userId);
    }
}
