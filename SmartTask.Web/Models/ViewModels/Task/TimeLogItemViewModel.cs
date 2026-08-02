namespace SmartTask.Web.Models.ViewModels.Task;

public class TimeLogItemViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public bool IsRunning => EndTime == null;
    public bool CanDelete { get; set; }
}