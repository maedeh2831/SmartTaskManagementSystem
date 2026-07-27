/*
| Module      : Database
| Entity      : ApplicationDbContext
| Purpose     : مدیریت ارتباط Entityها با پایگاه داده.
*/

using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SmartTask.Web.Data.Context
{
    public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Workspace
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }

        // Team
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        // Collaboration
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<TaskLabel> TaskLabels { get; set; }

        // Project
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Backlog> Backlogs { get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<SubTaskItem> SubTaskItems { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }

        // Tracking
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Soft Delete Filters

            modelBuilder.Entity<Workspace>()
                .HasQueryFilter(x => x.ViewState);

            modelBuilder.Entity<Project>()
                .HasQueryFilter(x => x.ViewState);

            modelBuilder.Entity<Team>()
                .HasQueryFilter(x => x.ViewState);

            modelBuilder.Entity<Comment>()
                .HasQueryFilter(x => x.ViewState);

            modelBuilder.Entity<TaskItem>()
                .HasQueryFilter(x => x.ViewState);
        }
    }
}