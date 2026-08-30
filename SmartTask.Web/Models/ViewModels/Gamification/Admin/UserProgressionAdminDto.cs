/*
| Module      : Gamification
| DTO         : UserProgressionAdminDto
| Purpose     : اطلاعات پیشرفت کاربر برای مدیران
*/

namespace SmartTask.Web.Models.ViewModels.Gamification.Admin
{
    public class UserProgressionAdminDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Level { get; set; }
        public int TotalExperience { get; set; }
        public int TotalPoints { get; set; }
        public int AvailablePoints { get; set; }
        public int TasksCompleted { get; set; }
        public int ProjectsCompleted { get; set; }
        public int AchievementsUnlocked { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int GlobalRank { get; set; }
        public bool RewardsSuspended { get; set; }
        public DateTime? SuspensionUntil { get; set; }
        public DateTime LastActivityDate { get; set; }
        public int AbuseReportsCount { get; set; }
        public int ConfirmedAbuseReports { get; set; }
    }
}
