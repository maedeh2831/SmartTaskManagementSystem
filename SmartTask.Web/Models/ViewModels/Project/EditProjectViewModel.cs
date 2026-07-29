using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Project;

public class EditProjectViewModel : IValidatableObject
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }

    [Required(ErrorMessage = "نام پروژه الزامی است.")]
    [StringLength(150, ErrorMessage = "نام پروژه نباید بیشتر از 150 کاراکتر باشد.")]
    [Display(Name = "نام پروژه")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "کلید پروژه الزامی است.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "کلید پروژه باید بین 2 تا 10 کاراکتر باشد.")]
    [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "کلید پروژه فقط باید شامل حروف انگلیسی و اعداد باشد.")]
    [Display(Name = "کلید پروژه")]
    public string Key { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "توضیحات نباید بیشتر از 1000 کاراکتر باشد.")]
    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "رنگ پروژه")]
    public string Color { get; set; } = "#4F46E5";

    [Display(Name = "آیکون")]
    public string Icon { get; set; } = "fa-solid fa-diagram-project";

    [DataType(DataType.Date)]
    [Display(Name = "تاریخ شروع")]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "موعد تحویل")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "وضعیت")]
    public ProjectStatusType Status { get; set; }

    [Display(Name = "اولویت")]
    public ProjectPriorityType Priority { get; set; }

    [Display(Name = "آرشیو شده")]
    public bool IsArchived { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && DueDate.HasValue && DueDate.Value < StartDate.Value)
        {
            yield return new ValidationResult(
                "موعد تحویل نمی‌تواند قبل از تاریخ شروع باشد.",
                new[] { nameof(DueDate) });
        }
    }
}