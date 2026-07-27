/*
| Module      : Workspace
| Entity      : WorkspaceInvitation
| Purpose     : مدیریت دعوت‌نامه‌های ارسال‌شده برای عضویت در Workspace (اعم از کاربران موجود و کاربران جدید).
*/
using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.Entities
{
    public class WorkspaceInvitation : BaseEntity
    {
        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public string Email { get; set; } = null!;

        // اگه ایمیل متعلق به یک کاربر ثبت‌نام‌شده باشه پر می‌شه
        public int? InvitedUserId { get; set; }
        public ApplicationUser? InvitedUser { get; set; }

        public WorkspaceRoleType Role { get; set; }

        public Guid Token { get; set; } = Guid.NewGuid();

        public WorkspaceInvitationStatusType Status { get; set; }
            = WorkspaceInvitationStatusType.Pending;

        public int InvitedByUserId { get; set; }
        public ApplicationUser InvitedByUser { get; set; } = null!;

        public DateTime ExpiryDate { get; set; }
        public DateTime? AcceptedDate { get; set; }
    }
}