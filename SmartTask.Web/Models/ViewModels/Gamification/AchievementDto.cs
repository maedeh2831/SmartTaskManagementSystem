/*
| Module      : ViewModels
| DTO         : AchievementDto
| Purpose     : نمایش اطلاعات دستاوردها
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class AchievementDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int Rarity { get; set; }
        public int Category { get; set; }
        public int RewardPoints { get; set; }
        public int RewardExperience { get; set; }
        public string? Condition { get; set; }
        public int ConditionValue { get; set; }

        /// <summary>آیا کاربر جاری این دستاورد را باز کرده است؟</summary>
        public bool IsUnlocked { get; set; }

        /// <summary>تاریخ باز شدن (در صورت باز بودن)</summary>
        public DateTime? UnlockedDate { get; set; }

        /// <summary>پیشرفت فعلی کاربر به سمت شرط دستاورد</summary>
        public int CurrentProgress { get; set; }

        /// <summary>درصد پیشرفت (۰ تا ۱۰۰)</summary>
        public int ProgressPercent { get; set; }
    }
}
