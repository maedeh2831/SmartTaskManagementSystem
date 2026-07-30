/*
| Module      : Agile
| Entity      : UserStory
| Purpose     : مدیریت Product Backlog Itemهای هر پروژه.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class UserStory : BaseEntity
    {
        #region Foreign Keys

        public int ProjectId { get; set; }

        public int BacklogId { get; set; }

        public int? SprintId { get; set; }

        public int? OwnerId { get; set; }

        #endregion

        #region Basic Information

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        /// <summary>
        /// معیارهای پذیرش (Acceptance Criteria)
        /// </summary>
        public string? AcceptanceCriteria { get; set; }

        #endregion

        #region Planning

        public int StoryPoint { get; set; }

        /// <summary>
        /// ارزش تجاری برای اولویت‌بندی Product Backlog
        /// </summary>
        public int BusinessValue { get; set; }

        /// <summary>
        /// ترتیب نمایش داخل Product Backlog
        /// </summary>
        public int Order { get; set; }

        public StoryPriorityType Priority { get; set; } = StoryPriorityType.Medium;

        public StoryStatusType Status { get; set; } = StoryStatusType.New;

        #endregion

        #region Navigation Properties

        public virtual Project Project { get; set; } = null!;

        public virtual Backlog Backlog { get; set; } = null!;

        public virtual Sprint? Sprint { get; set; }

        public virtual ApplicationUser? Owner { get; set; }

        public virtual ICollection<TaskItem> Tasks { get; set; } = new HashSet<TaskItem>();

        #endregion
    }
}