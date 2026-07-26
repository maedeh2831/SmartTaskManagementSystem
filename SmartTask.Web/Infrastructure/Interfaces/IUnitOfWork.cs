using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Infrastructure.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Workspace> Workspaces { get; }
        IGenericRepository<WorkspaceMember> WorkspaceMembers { get; }

        IGenericRepository<Team> Teams { get; }
        IGenericRepository<TeamMember> TeamMembers { get; }

        IGenericRepository<Project> Projects { get; }
        IGenericRepository<ProjectMember> ProjectMembers { get; }

        IGenericRepository<Sprint> Sprints { get; }
        IGenericRepository<Backlog> Backlogs { get; }
        IGenericRepository<UserStory> UserStories { get; }

        IGenericRepository<TaskItem> TaskItems { get; }
        IGenericRepository<SubTaskItem> SubTaskItems { get; }
        IGenericRepository<TaskAssignment> TaskAssignments { get; }

        IGenericRepository<Comment> Comments { get; }
        IGenericRepository<Attachment> Attachments { get; }

        IGenericRepository<Checklist> Checklists { get; }
        IGenericRepository<ChecklistItem> ChecklistItems { get; }

        IGenericRepository<Label> Labels { get; }
        IGenericRepository<TaskLabel> TaskLabels { get; }

        IGenericRepository<Reminder> Reminders { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<ActivityLog> ActivityLogs { get; }
        IGenericRepository<TimeLog> TimeLogs { get; }

        IGenericRepository<TaskDependency> TaskDependencies { get; }

        Task<int> SaveChangesAsync();
    }
}