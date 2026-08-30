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
        public DbSet<WorkspaceInvitation> WorkspaceInvitations { get; set; }

        // Team
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<ProjectTeam> ProjectTeams { get; set; }

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
        public DbSet<SprintReport> SprintReports { get; set; }
        public DbSet<Backlog> Backlogs { get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<SubTaskItem> SubTaskItems { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<OffroadTask> OffroadTasks { get; set; }
        public DbSet<OverdueCascadeLog> OverdueCascadeLogs { get; set; }
        public DbSet<TaskTradeRequest> TaskTradeRequests { get; set; }

        // Chat
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatReadState> ChatReadStates { get; set; }
        public DbSet<ChatMessageReaction> ChatMessageReactions { get; set; }

        // Tracking
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotificationPreference> UserNotificationPreferences { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        // Gamification
        public DbSet<UserProgression> UserProgressions { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<UserMilestoneProgress> UserMilestoneProgresses { get; set; }

        // Marketplace
        public DbSet<MarketplaceItem> MarketplaceItems { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }
        public DbSet<MarketplaceTransaction> MarketplaceTransactions { get; set; }

        // Leaderboards
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<TeamLeaderboard> TeamLeaderboards { get; set; }

        // Productivity Metrics
        public DbSet<ProductivityMetrics> ProductivityMetrics { get; set; }
        public DbSet<ProductivityScoreHistory> ProductivityScoreHistories { get; set; }

        // Phase 5: Advanced Features
        public DbSet<UserStreak> UserStreaks { get; set; }
        public DbSet<SeasonalEvent> SeasonalEvents { get; set; }
        public DbSet<UserSeasonalEventProgress> UserSeasonalEventProgresses { get; set; }
        public DbSet<AbuseReport> AbuseReports { get; set; }

        // Project Simulation
        public DbSet<ProjectSimulation> ProjectSimulations { get; set; }
        public DbSet<SimulationScenario> SimulationScenarios { get; set; }

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

            modelBuilder.Entity<Sprint>()
               .HasQueryFilter(x => x.ViewState);

            modelBuilder.Entity<ChatMessage>()
               .HasQueryFilter(x => x.ViewState);
        }
    }
}