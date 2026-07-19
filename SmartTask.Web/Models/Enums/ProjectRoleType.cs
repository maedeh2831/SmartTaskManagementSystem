/*
| Module      : Definitions
| Entity      : ProjectRoleType
| Purpose     : تعیین نقش اعضای پروژه.
*/

using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums
{
    public enum ProjectRoleType
    {
        [Display(Name = "مالک پروژه")]
        Owner = 1,

        [Display(Name = "مدیر پروژه")]
        Manager = 2,

        [Display(Name = "توسعه‌دهنده")]
        Developer = 3,

        [Display(Name = "تستر")]
        Tester = 4,

        [Display(Name = "مهمان")]
        Guest = 5
    }
}