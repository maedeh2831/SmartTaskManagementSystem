using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.UserStory;

public class CreateUserStoryViewModel
{
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
    [Range(0, 100, ErrorMessage = "Story Point باید بین ۰ تا ۱۰۰ باشد.")]
    public int StoryPoint { get; set; }

    [Display(Name = "ارزش تجاری")]
    [Range(0, 100, ErrorMessage = "ارزش تجاری باید بین ۰ تا ۱۰۰ باشد.")]
    public int BusinessValue { get; set; }

    [Display(Name = "اولویت")]
    public SmartTask.Web.Models.Enums.StoryPriorityType Priority { get; set; } =
        SmartTask.Web.Models.Enums.StoryPriorityType.Medium;
}