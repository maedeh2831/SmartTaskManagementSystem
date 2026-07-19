/*
| Module      : Definitions
| Entity      : ProjectStatusType
| Purpose     : تعیین وضعیت پروژه.
*/

using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums
{
    public enum ProjectStatusType
    {
        [Display(Name = "در حال برنامه‌ریزی")]
        Planning = 1,

        [Display(Name = "فعال")]
        Active = 2,

        [Display(Name = "متوقف شده")]
        OnHold = 3,

        [Display(Name = "تکمیل شده")]
        Completed = 4,

        [Display(Name = "لغو شده")]
        Cancelled = 5
    }
}