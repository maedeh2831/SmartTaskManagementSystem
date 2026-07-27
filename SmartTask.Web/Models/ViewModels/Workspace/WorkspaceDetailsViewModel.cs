using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class WorkspaceDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? Logo { get; set; }

    public VisibilityType Visibility { get; set; }

    public DateTime CreateDate { get; set; }

    public string OwnerName { get; set; } = "-";

    public int MembersCount { get; set; }

    public int ProjectsCount { get; set; }

    public int TasksCount { get; set; }

    public bool IsOwner { get; set; }
}