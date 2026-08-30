/*
| Module      : Gamification
| DTO         : LeaderboardUserContextDto
| Purpose     : نمایش رتبه کاربر و همسایگان آن
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class LeaderboardUserContextDto
    {
        public LeaderboardEntryDto CurrentUser { get; set; } = null!;
        public List<LeaderboardEntryDto> Neighbors { get; set; } = new List<LeaderboardEntryDto>(); // 2 above, 2 below
        public int TotalUsersInLeaderboard { get; set; }
    }
}
