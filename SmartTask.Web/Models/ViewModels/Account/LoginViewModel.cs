using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "رمز عبور الزامی است.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; } = null!;

        [Display(Name = "مرا به خاطر بسپار")]
        public bool RememberMe { get; set; }
    }
}