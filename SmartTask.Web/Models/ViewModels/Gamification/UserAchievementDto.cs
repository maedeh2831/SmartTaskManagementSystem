/*
| Module      : ViewModels
| DTO         : UserAchievementDto
| Purpose     : نمایش دستاوردهای آنلاک‌شده کاربر
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class UserAchievementDto
    {
        public int AchievementId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int Rarity { get; set; }
        public int Category { get; set; }
        public int RewardPoints { get; set; }
        public int RewardExperience { get; set; }
        public DateTime UnlockedDate { get; set; }
    }
}
