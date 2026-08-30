/*
| Module      : Gamification
| Interface   : IEquippedCosmeticsService
| Purpose     : خواندن اقلام فعال کاربر برای اعمال واقعی در ظاهر برنامه
*/

using SmartTask.Web.Models.ViewModels.Gamification;

namespace SmartTask.Web.Services.Gamification
{
    public interface IEquippedCosmeticsService
    {
        /// <summary>
        /// دریافت اقلام فعال کاربر (حاشیه آواتار، نشان، پوسته، مزایا)
        /// </summary>
        Task<EquippedCosmeticsDto> GetForUserAsync(int userId);

        /// <summary>
        /// آیا کاربر مزیت مشخصی را فعال کرده است؟ (مثل Double XP Boost)
        /// </summary>
        Task<bool> HasActivePerkAsync(int userId, string perkName);

        /// <summary>
        /// ضریب تجربه بر اساس مزایای فعال کاربر
        /// </summary>
        Task<double> GetExperienceMultiplierAsync(int userId);
    }
}
