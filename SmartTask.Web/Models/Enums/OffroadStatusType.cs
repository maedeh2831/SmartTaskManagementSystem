using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums;

public enum OffroadStatusType
{
    [Display(Name = "برای انجام")]
    ToDo = 1,

    [Display(Name = "درحال انجام")]
    InProgress = 2,

    [Display(Name = "انجام‌شده")]
    Done = 3
}