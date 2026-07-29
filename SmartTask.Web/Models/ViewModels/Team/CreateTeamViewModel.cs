using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Team;

public class CreateTeamViewModel
{
    public int WorkspaceId { get; set; }

    [Display(Name = "نام تیم")]
    [Required(ErrorMessage = "نام تیم الزامی است.")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Display(Name = "توضیحات")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "رنگ تیم")]
    public string Color { get; set; } = "#4F46E5";

    [Display(Name = "تیم خصوصی")]
    public bool IsPrivate { get; set; }
}