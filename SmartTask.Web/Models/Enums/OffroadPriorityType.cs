using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums;

public enum OffroadPriorityType
{
    [Display(Name = "کم")]
    Low = 1,

    [Display(Name = "معمولی")]
    Normal = 2,

    [Display(Name = "فوری")]
    Urgent = 3
}