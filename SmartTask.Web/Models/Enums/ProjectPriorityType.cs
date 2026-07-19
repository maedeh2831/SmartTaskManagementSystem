/*
| Module      : Definitions
| Entity      : ProjectPriorityType
| Purpose     : تعیین سطح اولویت پروژه.
*/

using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums
{
    public enum ProjectPriorityType
    {
        [Display(Name = "کم")]
        Low = 1,

        [Display(Name = "متوسط")]
        Medium = 2,

        [Display(Name = "زیاد")]
        High = 3,

        [Display(Name = "بحرانی")]
        Critical = 4
    }
}