using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Task;

public class EditTaskViewModel
{
    public int Id { get; set; }
    public int UserStoryId { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "عنوان Task الزامی است.")]
    [StringLength(250)]
    public string Title { get; set; } = null!;

    [Display(Name = "توضیحات")]
    [StringLength(4000)]
    public string? Description { get; set; }

    [Display(Name = "نوع")]
    public TaskType Type { get; set; }

    [Display(Name = "اولویت")]
    public TaskPriorityType Priority { get; set; }

    [Display(Name = "وضعیت")]
    public TaskStatusType Status { get; set; }

    [Display(Name = "برآورد (ساعت)")]
    [Range(0, 999)]
    public int Estimate { get; set; }

    [Display(Name = "تاریخ شروع")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "موعد انجام")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }
}