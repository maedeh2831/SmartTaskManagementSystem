using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartTask.Web.Models.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "نام الزامی است.")]
        [Display(Name = "نام")]
        [StringLength(50, ErrorMessage = "نام نمی‌تواند بیشتر از 50 کاراکتر باشد.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
        [Display(Name = "نام خانوادگی")]
        [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد.")]
        public string LastName { get; set; } = "";

        [Display(Name = "ایمیل")]
        public string Email { get; set; } = "";

        [Display(Name = "عنوان شغلی")]
        [StringLength(100, ErrorMessage = "عنوان شغلی نمی‌تواند بیشتر از 100 کاراکتر باشد.")]
        public string? JobTitle { get; set; }

        [Display(Name = "درباره من")]
        [StringLength(500, ErrorMessage = "متن درباره من نمی‌تواند بیشتر از 500 کاراکتر باشد.")]
        public string? Bio { get; set; }

        [Display(Name = "تصویر پروفایل")]

        public string? Avatar { get; set; }

        public string AvatarUrl =>
            string.IsNullOrWhiteSpace(Avatar)
                ? "/images/default-avatar.svg"
                : Avatar;

        public IFormFile? NewAvatar { get; set; }
    }
}