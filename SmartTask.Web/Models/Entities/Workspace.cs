/*
| Module      : Workspace
| Entity      : Workspace
| Purpose     : نگهداری اطلاعات Workspace و مدیریت مالک، اعضا، پروژه‌ها و تیم‌ها.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities;

public class Workspace
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Logo { get; set; }

    public string? Color { get; set; }

    public VisibilityType Visibility { get; set; }
        = VisibilityType.Private;

    public bool IsActive { get; set; } = true;

    public DateTime CreateDate { get; set; } = DateTime.Now;

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public bool ViewState { get; set; } = true;

    //================ Owner =================

    public int OwnerId { get; set; }

    public virtual ApplicationUser Owner { get; set; } = null!;

    //================ Navigation =================

    public virtual ICollection<Project> Projects { get; set; }
        = new HashSet<Project>();

    public virtual ICollection<Team> Teams { get; set; }
        = new HashSet<Team>();

    public virtual ICollection<WorkspaceMember> Members { get; set; }
        = new HashSet<WorkspaceMember>();
}