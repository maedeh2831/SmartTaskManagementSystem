using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Backlog;

public class CreateQuickStoryViewModel
{
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "عنوان User Story الزامی است.")]
    [StringLength(250, ErrorMessage = "عنوان نباید بیشتر از 250 کاراکتر باشد.")]
    public string Title { get; set; } = null!;
}