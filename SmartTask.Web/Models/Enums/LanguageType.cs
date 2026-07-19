/*
| Module      : Definitions
| Entity      : LanguageType
| Purpose     : تعیین زبان‌های قابل پشتیبانی در سامانه.
*/

using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums
{
    public enum LanguageType
    {
        [Display(Name = "فارسی")]
        Persian = 1,

        [Display(Name = "English")]
        English = 2
    }
}