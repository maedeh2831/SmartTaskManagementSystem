/*
| Module      : Database
| Entity      : ApplicationDbContext
| Purpose     : مدیریت ارتباط Entityها با پایگاه داده.
*/

using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Identity
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        // Workspace
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }

        // Team
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        // Project
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Backlog> Backlogs { get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<SubTaskItem> SubTaskItems { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}