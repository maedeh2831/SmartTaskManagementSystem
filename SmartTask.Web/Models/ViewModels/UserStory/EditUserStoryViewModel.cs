using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.UserStory;

public class EditUserStoryViewModel
{
    public int Id { get; set; }
    public int ProjectId { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "عنوان User Story الزامی است.")]
    [StringLength(250)]
    public string Title { get; set; } = null!;

    [Display(Name = "توضیحات")]
    [StringLength(3000)]
    public string? Description { get; set; }

    [Display(Name = "معیارهای پذیرش")]
    [StringLength(3000)]
    public string? AcceptanceCriteria { get; set; }

    [Display(Name = "Story Point")]
    [Range(0, 100)]
    public int StoryPoint { get; set; }

    [Display(Name = "ارزش تجاری")]
    [Range(0, 100)]
    public int BusinessValue { get; set; }

    [Display(Name = "اولویت")]
    public StoryPriorityType Priority { get; set; }

    [Display(Name = "وضعیت")]
    public StoryStatusType Status { get; set; }
}