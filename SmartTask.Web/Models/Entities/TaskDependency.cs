/*
| Module      : Tracking
| Entity      : TaskDependency
| Purpose     : مدیریت وابستگی بین Taskها.
*/


namespace SmartTask.Web.Models.Entities
{
    public class TaskDependency : BaseEntity
    {
        /// <summary>
        /// Task اصلی
        /// </summary>
        public int TaskItemId { get; set; }

        /// <summary>
        /// Task وابسته
        /// </summary>
        public int DependsOnTaskItemId { get; set; }

        /// <summary>
        /// آیا وابستگی اجباری است؟
        /// </summary>
        public bool IsRequired { get; set; }

        // Navigation Properties

        public virtual TaskItem TaskItem { get; set; } = null!;

        public virtual TaskItem DependsOnTaskItem { get; set; } = null!;
    }
}