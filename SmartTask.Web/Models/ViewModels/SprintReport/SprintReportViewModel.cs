namespace SmartTask.Web.Models.ViewModels.SprintReport;

public class SprintReportViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public string GeneratedByName { get; set; } = null!;
    public DateTime GeneratedDate { get; set; }
}