using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Repositories

        private IGenericRepository<Workspace>? _workspaces;
        private IGenericRepository<WorkspaceMember>? _workspaceMembers;

        private IGenericRepository<Team>? _teams;
        private IGenericRepository<TeamMember>? _teamMembers;

        private IGenericRepository<Project>? _projects;
        private IGenericRepository<ProjectMember>? _projectMembers;

        private IGenericRepository<Sprint>? _sprints;
        private IGenericRepository<Backlog>? _backlogs;
        private IGenericRepository<UserStory>? _userStories;

        private IGenericRepository<TaskItem>? _taskItems;
        private IGenericRepository<SubTaskItem>? _subTaskItems;
        private IGenericRepository<TaskAssignment>? _taskAssignments;

        private IGenericRepository<Comment>? _comments;
        private IGenericRepository<Attachment>? _attachments;

        private IGenericRepository<Checklist>? _checklists;
        private IGenericRepository<ChecklistItem>? _checklistItems;

        private IGenericRepository<Label>? _labels;
        private IGenericRepository<TaskLabel>? _taskLabels;

        private IGenericRepository<Reminder>? _reminders;
        private IGenericRepository<Notification>? _notifications;
        private IGenericRepository<ActivityLog>? _activityLogs;
        private IGenericRepository<TimeLog>? _timeLogs;

        private IGenericRepository<TaskDependency>? _taskDependencies;

        #endregion

        #region Repository Properties

        public IGenericRepository<Workspace> Workspaces
            => _workspaces ??= new GenericRepository<Workspace>(_context);

        public IGenericRepository<WorkspaceMember> WorkspaceMembers
            => _workspaceMembers ??= new GenericRepository<WorkspaceMember>(_context);

        public IGenericRepository<Team> Teams
            => _teams ??= new GenericRepository<Team>(_context);

        public IGenericRepository<TeamMember> TeamMembers
            => _teamMembers ??= new GenericRepository<TeamMember>(_context);

        public IGenericRepository<Project> Projects
            => _projects ??= new GenericRepository<Project>(_context);

        public IGenericRepository<ProjectMember> ProjectMembers
            => _projectMembers ??= new GenericRepository<ProjectMember>(_context);

        public IGenericRepository<Sprint> Sprints
            => _sprints ??= new GenericRepository<Sprint>(_context);

        public IGenericRepository<Backlog> Backlogs
            => _backlogs ??= new GenericRepository<Backlog>(_context);

        public IGenericRepository<UserStory> UserStories
            => _userStories ??= new GenericRepository<UserStory>(_context);

        public IGenericRepository<TaskItem> TaskItems
            => _taskItems ??= new GenericRepository<TaskItem>(_context);

        public IGenericRepository<SubTaskItem> SubTaskItems
            => _subTaskItems ??= new GenericRepository<SubTaskItem>(_context);

        public IGenericRepository<TaskAssignment> TaskAssignments
            => _taskAssignments ??= new GenericRepository<TaskAssignment>(_context);

        public IGenericRepository<Comment> Comments
            => _comments ??= new GenericRepository<Comment>(_context);

        public IGenericRepository<Attachment> Attachments
            => _attachments ??= new GenericRepository<Attachment>(_context);

        public IGenericRepository<Checklist> Checklists
            => _checklists ??= new GenericRepository<Checklist>(_context);

        public IGenericRepository<ChecklistItem> ChecklistItems
            => _checklistItems ??= new GenericRepository<ChecklistItem>(_context);

        public IGenericRepository<Label> Labels
            => _labels ??= new GenericRepository<Label>(_context);

        public IGenericRepository<TaskLabel> TaskLabels
            => _taskLabels ??= new GenericRepository<TaskLabel>(_context);

        public IGenericRepository<Reminder> Reminders
            => _reminders ??= new GenericRepository<Reminder>(_context);

        public IGenericRepository<Notification> Notifications
            => _notifications ??= new GenericRepository<Notification>(_context);

        public IGenericRepository<ActivityLog> ActivityLogs
            => _activityLogs ??= new GenericRepository<ActivityLog>(_context);

        public IGenericRepository<TimeLog> TimeLogs
            => _timeLogs ??= new GenericRepository<TimeLog>(_context);

        public IGenericRepository<TaskDependency> TaskDependencies
            => _taskDependencies ??= new GenericRepository<TaskDependency>(_context);

        #endregion

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}