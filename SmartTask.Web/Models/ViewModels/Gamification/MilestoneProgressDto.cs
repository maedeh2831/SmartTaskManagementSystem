/*
| Module      : ViewModels
| DTO         : MilestoneProgressDto
| Purpose     : نمایش پیشرفت در رسیدن به نقاط عطف
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class MilestoneProgressDto
    {
        public int MilestoneId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int Type { get; set; }
        public int CurrentProgress { get; set; }
        public int TargetValue { get; set; }
        public int CompletionPercentage { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int RewardPoints { get; set; }
        public int RewardExperience { get; set; }
    }
}
