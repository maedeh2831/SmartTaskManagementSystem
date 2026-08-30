/*
| Module      : Gamification
| DTO         : TeamLeaderboardDto
| Purpose     : نمایش اطلاعات رتبه‌بندی تیم
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class TeamLeaderboardDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public string? TeamLogo { get; set; }
        public string? TeamColor { get; set; }

        public int TeamRank { get; set; }
        public int TotalTeamPoints { get; set; }
        public int AverageTeamLevel { get; set; }
        public int TotalTeamExperience { get; set; }

        public int TasksCompleted { get; set; }
        public int ProjectsCompleted { get; set; }
        public int TeamMemberCount { get; set; }
        public int AchievementsUnlocked { get; set; }

        public int WeeklyPoints { get; set; }
        public int MonthlyPoints { get; set; }

        public double AverageCompletionRate { get; set; }
        public double AverageProductivity { get; set; }
        public int ActiveMembersThisWeek { get; set; }

        public int RankChangeFromPrevious { get; set; }
        public DateTime LastUpdated { get; set; }

        // Member summary
        public List<TeamMemberSummaryDto> TopMembers { get; set; } = new List<TeamMemberSummaryDto>();
    }

    public class TeamMemberSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int CurrentLevel { get; set; }
        public int TotalPoints { get; set; }
        public int Rank { get; set; }
    }
}
