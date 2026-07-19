/*
| Module      : Definitions
| Entity      : WorkspaceRoleType
| Purpose     : تعیین نقش کاربران در Workspace.
*/

using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums
{
    public enum WorkspaceRoleType
    {
        [Display(Name = "مالک")]
        Owner = 1,

        [Display(Name = "مدیر")]
        Admin = 2,

        [Display(Name = "مدیر پروژه")]
        ProjectManager = 3,

        [Display(Name = "توسعه‌دهنده")]
        Developer = 4,

        [Display(Name = "تستر")]
        Tester = 5,

        [Display(Name = "مشاهده‌گر")]
        Viewer = 6
    }
}