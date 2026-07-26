using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "رمز عبور را وارد کنید")]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "تکرار رمز عبور را وارد کنید")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "رمز عبور یکسان نیست.")]
        public string ConfirmPassword { get; set; } = "";
    }
}