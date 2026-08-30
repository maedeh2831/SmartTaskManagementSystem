/*
| Module      : Gamification
| Interface   : IRewardEligibilityService
| Purpose     : جلوگیری از سوء‌استفاده پیش از اعطای پاداش (تسک تکراری، تسک جعلی، نرخ بالا)
*/

namespace SmartTask.Web.Services.Gamification
{
    /// <summary>دلیل رد یا تأیید پاداش</summary>
    public record RewardEligibility(bool IsAllowed, string? Reason = null)
    {
        public static RewardEligibility Allow() => new(true);
        public static RewardEligibility Deny(string reason) => new(false, reason);
    }

    public interface IRewardEligibilityService
    {
        /// <summary>
        /// بررسی می‌کند آیا کاربر برای تکمیل این تسک واجد شرایط پاداش است.
        /// از پاداش تکراری، تسک‌های جعلی و نرخ غیرعادی جلوگیری می‌کند.
        /// </summary>
        Task<RewardEligibility> CanRewardTaskAsync(int userId, int taskId);

        /// <summary>
        /// آیا برای این تسک قبلاً به این کاربر پاداش داده شده است؟
        /// </summary>
        Task<bool> HasAlreadyRewardedTaskAsync(int userId, int taskId);
    }
}
