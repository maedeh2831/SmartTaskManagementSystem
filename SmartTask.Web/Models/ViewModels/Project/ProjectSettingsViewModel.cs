using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Project;

public class ProjectSettingsViewModel
{
    public int Id { get; set; }

    public int WorkspaceId { get; set; }

    [Required(ErrorMessage = "نام پروژه الزامی است.")]
    public string Name { get; set; } = null!;

    public string Key { get; set; } = null!;

    [Required(ErrorMessage = "انتخاب رنگ الزامی است.")]
    public string Color { get; set; } = "#4F46E5";

    [Required(ErrorMessage = "انتخاب آیکون الزامی است.")]
    public string Icon { get; set; } = "fa-solid fa-diagram-project";

    public bool IsArchived { get; set; }
}