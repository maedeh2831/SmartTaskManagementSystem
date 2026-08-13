using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "رمز عبور فعلی را وارد کنید.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور فعلی")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "رمز عبور جدید را وارد کنید.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد.")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور جدید")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "تکرار رمز عبور الزامی است.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "تکرار رمز عبور با رمز جدید یکسان نیست.")]
        [Display(Name = "تکرار رمز عبور جدید")]
        public string ConfirmPassword { get; set; } = "";
    }
}