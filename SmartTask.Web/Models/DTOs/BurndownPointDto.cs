namespace SmartTask.Web.Models.DTOs;

public class BurndownPointDto
{
    public DateTime Date { get; set; }
    public int IdealRemaining { get; set; }
    public int? ActualRemaining { get; set; }
}