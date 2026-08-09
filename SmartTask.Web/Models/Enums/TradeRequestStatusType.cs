using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.Enums;

public enum TradeRequestStatusType
{
    [Display(Name = "در انتظار")]
    Pending = 1,

    [Display(Name = "تأییدشده")]
    Accepted = 2,

    [Display(Name = "ردشده")]
    Rejected = 3,

    [Display(Name = "لغوشده")]
    Cancelled = 4
}