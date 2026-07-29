/*
| Module      : Project
| Entity      : ProjectTeam
| Purpose     : جدول واسط رابطه چند‌به‌چند بین Project و Team.
*/

namespace SmartTask.Web.Models.Entities
{
    public class ProjectTeam : BaseEntity
    {
        public int ProjectId { get; set; }

        public int TeamId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public Project Project { get; set; } = null!;

        public Team Team { get; set; } = null!;
    }
}