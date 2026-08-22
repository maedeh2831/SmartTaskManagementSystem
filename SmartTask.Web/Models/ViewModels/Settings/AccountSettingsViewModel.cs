using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.ViewModels.Settings
{
    public class AccountSettingsViewModel
    {
        [Display(Name = "منطقه زمانی")]
        public string TimeZone { get; set; } = "Asia/Tehran";

        [Display(Name = "فرمت تاریخ")]
        public DateFormatType DateFormat { get; set; }

    }
}