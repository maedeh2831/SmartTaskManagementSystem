using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.ViewModels.Settings
{
    public class AccountSettingsViewModel
    {
        [Display(Name = "زبان سیستم")]
        public LanguageType Language { get; set; }

        [Display(Name = "منطقه زمانی")]
        public string TimeZone { get; set; } = "Asia/Tehran";

        [Display(Name = "فرمت تاریخ")]
        public DateFormatType DateFormat { get; set; }

        [Display(Name = "نمایش نام کامل")]
        public bool ShowFullName { get; set; }
    }
}