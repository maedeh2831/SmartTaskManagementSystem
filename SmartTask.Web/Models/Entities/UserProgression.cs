/*
| Module      : Gamification
| Entity      : UserProgression
| Purpose     : ردیابی پیشرفت و سطح کاربر
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserProgression : BaseEntity
    {
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int CurrentLevel { get; set; } = 1;
        public int TotalExperience { get; set; } = 0;
        public int ExperienceForNextLevel { get; set; } = 1000;

        public int TasksCompleted { get; set; } = 0;
        public int ProjectsCompleted { get; set; } = 0;
        public int SprintsCompleted { get; set; } = 0;

        public DateTime LastProgressUpdate { get; set; } = DateTime.UtcNow;
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
