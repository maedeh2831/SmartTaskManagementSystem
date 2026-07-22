using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "وارد کردن نام الزامی است.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "وارد کردن نام خانوادگی الزامی است.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "وارد کردن نام کاربری الزامی است.")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "وارد کردن ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "وارد کردن رمز عبور الزامی است.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تکرار رمز عبور الزامی است.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "تکرار رمز عبور صحیح نیست.")]
        [Display(Name = "تکرار رمز عبور")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}