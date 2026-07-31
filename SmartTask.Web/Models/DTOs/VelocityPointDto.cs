namespace SmartTask.Web.Models.DTOs;

public class VelocityPointDto
{
    public string SprintName { get; set; } = null!;
    public int PlannedPoints { get; set; }
    public int CompletedPoints { get; set; }
}