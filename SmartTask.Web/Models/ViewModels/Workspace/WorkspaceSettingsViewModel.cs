/*
| Module      : Workspace
| ViewModel   : WorkspaceSettingsViewModel
| Purpose     : مدیریت تنظیمات فضای کاری (لوگو، رنگ، منطقه زمانی).
*/
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SmartTask.Web.Common.Attributes;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class WorkspaceSettingsViewModel
{
    public int Id { get; set; }

    [Display(Name = "نام فضای کاری")]
    public string Name { get; set; } = null!;

    [Display(Name = "لوگوی فعلی")]
    public string? CurrentLogo { get; set; }

    [Display(Name = "آپلود لوگوی جدید")]
    [MaxFileSize(2 * 1024 * 1024, ErrorMessage = "حجم فایل نباید بیشتر از ۲ مگابایت باشد.")]
    [AllowedExtensions(new[] { ".jpg", ".jpeg", ".png", ".webp" }, ErrorMessage = "فقط فرمت‌های jpg، png و webp مجاز است.")]
    public IFormFile? LogoFile { get; set; }

    [Display(Name = "رنگ تم")]
    [Required(ErrorMessage = "انتخاب رنگ الزامی است.")]
    [RegularExpression("^#([A-Fa-f0-9]{6})$", ErrorMessage = "کد رنگ معتبر نیست.")]
    public string Color { get; set; } = "#4F46E5";

    [Display(Name = "منطقه زمانی")]
    [Required(ErrorMessage = "انتخاب منطقه زمانی الزامی است.")]
    public string TimeZone { get; set; } = "Iran Standard Time";
}