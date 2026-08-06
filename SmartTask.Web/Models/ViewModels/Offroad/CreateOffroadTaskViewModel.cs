using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Offroad;

public class CreateOffroadTaskViewModel
{
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "عنوان الزامی است.")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    public OffroadPriorityType Priority { get; set; } = OffroadPriorityType.Normal;

    public DateTime? DueDate { get; set; }

    public int? AssignedToUserId { get; set; }
}