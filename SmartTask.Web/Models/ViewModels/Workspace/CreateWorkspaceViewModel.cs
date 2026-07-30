using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class CreateWorkspaceViewModel
{

    [Display(Name = "نام فضای کاری")]
    [Required(ErrorMessage = "نام فضای کاری الزامی است.")]
    [StringLength(100)]
    public string Name { get; set; } = null!;



    [Display(Name = "توضیحات")]
    [StringLength(500)]
    public string? Description { get; set; }



    [Display(Name = "رنگ Workspace")]
    public string Color { get; set; }
        = "#4F46E5";



    [Display(Name = "سطح دسترسی")]
    public VisibilityType Visibility { get; set; }
        = VisibilityType.Private;

}