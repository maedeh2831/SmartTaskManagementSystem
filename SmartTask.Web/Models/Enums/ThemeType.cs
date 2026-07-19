/*
| Module      : Definitions
| Entity      : ThemeType
| Purpose     : تعیین قالب‌های ظاهری قابل استفاده در سامانه.
*/

using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums
{
    public enum ThemeType
    {
        [Display(Name = "روشن")]
        Light = 1,

        [Display(Name = "تیره")]
        Dark = 2
    }
}