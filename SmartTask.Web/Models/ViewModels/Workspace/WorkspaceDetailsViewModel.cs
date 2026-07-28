using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class WorkspaceDetailsViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Logo { get; set; }

    public string? Color { get; set; }

    public bool IsActive { get; set; }

    public VisibilityType Visibility { get; set; }

    public string OwnerName { get; set; } = "";

    public int ProjectCount { get; set; }

    public int TeamCount { get; set; }

    public int MemberCount { get; set; }

    public DateTime CreateDate { get; set; }
}