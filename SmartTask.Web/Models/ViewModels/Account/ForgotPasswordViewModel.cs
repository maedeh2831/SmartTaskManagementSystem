using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "وارد کردن ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; } = string.Empty;
    }
}