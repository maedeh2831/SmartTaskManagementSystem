/*
| Module      : Definitions
| Entity      : TaskDensityType
| Purpose     : تعیین میزان تراکم نمایش Taskها در لیست‌ها و بردها.
*/
using System.ComponentModel.DataAnnotations;
namespace SmartTask.Web.Models.Enums
{
    public enum TaskDensityType
    {
        [Display(Name = "فشرده")]
        Compact = 1,
        [Display(Name = "راحت")]
        Comfortable = 2
    }
}