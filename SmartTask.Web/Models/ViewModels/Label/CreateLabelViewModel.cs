namespace SmartTask.Web.Models.ViewModels.Label;

public class CreateLabelViewModel
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string? Color { get; set; }
}