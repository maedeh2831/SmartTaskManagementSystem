/*
| Module      : Collaboration
| Entity      : Attachment
| Purpose     : نگهداری فایل‌های پیوست شده به Task.
*/

namespace SmartTask.Web.Models.Entities
{
    public class Attachment : BaseEntity
    {
        // Properties

        public int TaskItemId { get; set; }

        public int ApplicationUserId { get; set; }

        public string FileName { get; set; } = null!;

        public string FilePath { get; set; } = null!;

        public long FileSize { get; set; }

        public string ContentType { get; set; } = null!;

        // Navigation Properties

        public TaskItem TaskItem { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}