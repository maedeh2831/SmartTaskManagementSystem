namespace SmartTask.Web.Models.ViewModels.Team;

public class TeamListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Color { get; set; } = "#4F46E5";
    public string? Logo { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsArchived { get; set; }
    public int MembersCount { get; set; }
    public int ProjectsCount { get; set; }
    public DateTime CreateDate { get; set; }
}