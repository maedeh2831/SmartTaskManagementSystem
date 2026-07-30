using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Sprint;

public class CreateSprintViewModel : IValidatableObject
{
    public int ProjectId { get; set; }

    [Display(Name = "نام اسپرینت")]
    [Required(ErrorMessage = "نام اسپرینت الزامی است.")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Display(Name = "هدف اسپرینت")]
    [StringLength(1000)]
    public string? Goal { get; set; }

    [Display(Name = "تاریخ شروع")]
    [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "تاریخ پایان")]
    [Required(ErrorMessage = "تاریخ پایان الزامی است.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(14);

    [Display(Name = "ظرفیت (Story Point)")]
    [Required(ErrorMessage = "ظرفیت اسپرینت الزامی است.")]
    [Range(1, 999, ErrorMessage = "ظرفیت باید بین ۱ تا ۹۹۹ باشد.")]
    public int Capacity { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "تاریخ پایان باید بعد از تاریخ شروع باشد.",
                new[] { nameof(EndDate) });
        }
    }
}