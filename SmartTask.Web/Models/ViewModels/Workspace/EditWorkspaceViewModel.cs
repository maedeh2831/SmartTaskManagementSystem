using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class EditWorkspaceViewModel
{
    public int Id { get; set; }

    [Display(Name = "نام فضای کاری")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Display(Name = "توضیحات")]
    [StringLength(500)]
    public string? Description { get; set; }

    public string? Logo { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; }

    public VisibilityType Visibility { get; set; }
}