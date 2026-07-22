/*
| Module      : Agile
| Entity      : TaskItem
| Purpose     : مدیریت وظایف اجرایی هر User Story.
*/

using SmartTask.Web.Models.Enums;
using System.Net.Mail;

namespace SmartTask.Web.Models.Entities
{
    public class TaskItem : BaseEntity
    {
        // Properties

        public int UserStoryId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public TaskStatusType Status { get; set; } = TaskStatusType.ToDo;

        public TaskPriorityType Priority { get; set; } = TaskPriorityType.Medium;

        public TaskType Type { get; set; } = TaskType.Task;

        public int Estimate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        // Navigation Properties

        public UserStory UserStory { get; set; } = null!;

        public ICollection<SubTaskItem> SubTasks { get; set; } = new List<SubTaskItem>();

        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        public ICollection<Checklist> Checklists { get; set; } = new List<Checklist>();

        public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();

        public virtual ICollection<Reminder> Reminders { get; set; } = new HashSet<Reminder>();

        public ICollection<ActivityLog> ActivityLogs { get; set; } = new HashSet<ActivityLog>();

        public virtual ICollection<TimeLog> TimeLogs { get; set; } = new HashSet<TimeLog>();
    }
}