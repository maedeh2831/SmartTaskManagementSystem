/*
| Module      : Definitions
| Entity      : DateFormatType
| Purpose     : تعیین فرمت نمایش تاریخ در سامانه.
*/
using System.ComponentModel.DataAnnotations;
namespace SmartTask.Web.Models.Enums
{
    public enum DateFormatType
    {
        [Display(Name = "شمسی")]
        Jalali = 1,
        [Display(Name = "میلادی")]
        Gregorian = 2
    }
}