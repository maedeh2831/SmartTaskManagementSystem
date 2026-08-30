using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class ApplyPendingGamificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_UserProgressions_UserProgressionId",
                table: "UserAchievements");

            migrationBuilder.CreateTable(
                name: "AbuseReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Evidence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeverityScore = table.Column<int>(type: "int", nullable: false),
                    ConfidenceLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RelatedTaskId = table.Column<int>(type: "int", nullable: true),
                    RelatedProjectId = table.Column<int>(type: "int", nullable: true),
                    IncidentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RewardsRefunded = table.Column<bool>(type: "bit", nullable: false),
                    RewardsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    RefundedAmount = table.Column<int>(type: "int", nullable: false),
                    SuspensionUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoDetectionRule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbuseReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbuseReports_ApplicationUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AbuseReports_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leaderboards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WorkspaceId = table.Column<int>(type: "int", nullable: true),
                    GlobalRank = table.Column<int>(type: "int", nullable: false),
                    WorkspaceRank = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TotalExperience = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TasksCompleted = table.Column<int>(type: "int", nullable: false),
                    ProjectsCompleted = table.Column<int>(type: "int", nullable: false),
                    AchievementsUnlocked = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveCompletionDays = table.Column<int>(type: "int", nullable: false),
                    WeeklyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MonthlyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WeeklyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthlyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RankChangeFromPrevious = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaderboards_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leaderboards_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    TotalSold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsLimitedTime = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvailableUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    RewardPoints = table.Column<int>(type: "int", nullable: false),
                    RewardExperience = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSimulations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BaselineStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaselineEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTasksCount = table.Column<int>(type: "int", nullable: false),
                    CriticalPathCalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriticalPathLengthDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSimulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSimulations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeasonalEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AchievementBonusMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RewardBonusMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraPointsPerCompletion = table.Column<int>(type: "int", nullable: false),
                    EligibilityCriteria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    CurrentParticipants = table.Column<int>(type: "int", nullable: false),
                    HasEventLeaderboard = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonalEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamLeaderboards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    TeamRank = table.Column<int>(type: "int", nullable: false),
                    TotalTeamPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AverageTeamLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TotalTeamExperience = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TasksCompleted = table.Column<int>(type: "int", nullable: false),
                    ProjectsCompleted = table.Column<int>(type: "int", nullable: false),
                    TeamMemberCount = table.Column<int>(type: "int", nullable: false),
                    AchievementsUnlocked = table.Column<int>(type: "int", nullable: false),
                    WeeklyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MonthlyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WeeklyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthlyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AverageCompletionRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    AverageProductivity = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    ActiveMembersThisWeek = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RankChangeFromPrevious = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamLeaderboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamLeaderboards_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamLeaderboards_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserStreaks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<int>(type: "int", nullable: false),
                    StreakStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Milestone3Days = table.Column<bool>(type: "bit", nullable: false),
                    Milestone7Days = table.Column<bool>(type: "bit", nullable: false),
                    Milestone14Days = table.Column<bool>(type: "bit", nullable: false),
                    Milestone30Days = table.Column<bool>(type: "bit", nullable: false),
                    Milestone100Days = table.Column<bool>(type: "bit", nullable: false),
                    TasksCompletedToday = table.Column<int>(type: "int", nullable: false),
                    XpGainedToday = table.Column<int>(type: "int", nullable: false),
                    LastResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserTimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStreaks_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserWalletId = table.Column<int>(type: "int", nullable: false),
                    MarketplaceItemId = table.Column<int>(type: "int", nullable: false),
                    PointsSpent = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_MarketplaceItems_MarketplaceItemId",
                        column: x => x.MarketplaceItemId,
                        principalTable: "MarketplaceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_UserWallets_UserWalletId",
                        column: x => x.UserWalletId,
                        principalTable: "UserWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MarketplaceItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsEquipped = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AcquiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EquippedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInventories_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserInventories_MarketplaceItems_MarketplaceItemId",
                        column: x => x.MarketplaceItemId,
                        principalTable: "MarketplaceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMilestoneProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MilestoneId = table.Column<int>(type: "int", nullable: false),
                    CurrentProgress = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastProgressUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMilestoneProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMilestoneProgresses_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMilestoneProgresses_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SimulationScenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectSimulationId = table.Column<int>(type: "int", nullable: false),
                    SimulatedTaskId = table.Column<int>(type: "int", nullable: false),
                    ScenarioName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OriginalTaskEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DelayDays = table.Column<int>(type: "int", nullable: false),
                    NewProjectEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalProjectEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectDelayDays = table.Column<int>(type: "int", nullable: false),
                    TotalAffectedTasks = table.Column<int>(type: "int", nullable: false),
                    AffectedTasksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriticalPathJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Medium"),
                    SimulatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulationScenarios_ProjectSimulations_ProjectSimulationId",
                        column: x => x.ProjectSimulationId,
                        principalTable: "ProjectSimulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSeasonalEventProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SeasonalEventId = table.Column<int>(type: "int", nullable: false),
                    EventPoints = table.Column<int>(type: "int", nullable: false),
                    TasksCompleted = table.Column<int>(type: "int", nullable: false),
                    AchievementsUnlocked = table.Column<int>(type: "int", nullable: false),
                    CurrentRank = table.Column<int>(type: "int", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HasClaimed = table.Column<bool>(type: "bit", nullable: false),
                    ClaimedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSeasonalEventProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSeasonalEventProgresses_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSeasonalEventProgresses_SeasonalEvents_SeasonalEventId",
                        column: x => x.SeasonalEventId,
                        principalTable: "SeasonalEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_ReviewedByUserId",
                table: "AbuseReports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AbuseReports_UserId",
                table: "AbuseReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_GlobalRank",
                table: "Leaderboards",
                column: "GlobalRank");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_LastUpdated",
                table: "Leaderboards",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_TotalPoints",
                table: "Leaderboards",
                column: "TotalPoints");

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_UserId_WorkspaceId",
                table: "Leaderboards",
                columns: new[] { "UserId", "WorkspaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_WorkspaceId_WorkspaceRank",
                table: "Leaderboards",
                columns: new[] { "WorkspaceId", "WorkspaceRank" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItems_Category",
                table: "MarketplaceItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItems_IsActive",
                table: "MarketplaceItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItems_IsLimitedTime_AvailableFrom_AvailableUntil",
                table: "MarketplaceItems",
                columns: new[] { "IsLimitedTime", "AvailableFrom", "AvailableUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_MarketplaceItemId",
                table: "MarketplaceTransactions",
                column: "MarketplaceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_Status",
                table: "MarketplaceTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_TransactionDate",
                table: "MarketplaceTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_UserId",
                table: "MarketplaceTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_UserId_TransactionDate",
                table: "MarketplaceTransactions",
                columns: new[] { "UserId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_UserWalletId",
                table: "MarketplaceTransactions",
                column: "UserWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulations_CreatedDate",
                table: "ProjectSimulations",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulations_ProjectId",
                table: "ProjectSimulations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulations_ProjectId_CreatedDate",
                table: "ProjectSimulations",
                columns: new[] { "ProjectId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_ProjectSimulationId",
                table: "SimulationScenarios",
                column: "ProjectSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_ProjectSimulationId_SimulatedAt",
                table: "SimulationScenarios",
                columns: new[] { "ProjectSimulationId", "SimulatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_RiskLevel",
                table: "SimulationScenarios",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_SimulatedAt",
                table: "SimulationScenarios",
                column: "SimulatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_SimulatedTaskId",
                table: "SimulationScenarios",
                column: "SimulatedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamLeaderboard_LastUpdated",
                table: "TeamLeaderboards",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_TeamLeaderboard_TeamId",
                table: "TeamLeaderboards",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamLeaderboard_TotalTeamPoints",
                table: "TeamLeaderboards",
                column: "TotalTeamPoints");

            migrationBuilder.CreateIndex(
                name: "IX_TeamLeaderboard_WorkspaceId_TeamRank",
                table: "TeamLeaderboards",
                columns: new[] { "WorkspaceId", "TeamRank" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_IsEquipped",
                table: "UserInventories",
                column: "IsEquipped");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_MarketplaceItemId",
                table: "UserInventories",
                column: "MarketplaceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_UserId",
                table: "UserInventories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_UserId_MarketplaceItemId",
                table: "UserInventories",
                columns: new[] { "UserId", "MarketplaceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMilestoneProgresses_MilestoneId",
                table: "UserMilestoneProgresses",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMilestoneProgresses_UserId",
                table: "UserMilestoneProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSeasonalEventProgresses_SeasonalEventId",
                table: "UserSeasonalEventProgresses",
                column: "SeasonalEventId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSeasonalEventProgresses_UserId",
                table: "UserSeasonalEventProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStreaks_UserId",
                table: "UserStreaks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_UserProgressions_UserProgressionId",
                table: "UserAchievements",
                column: "UserProgressionId",
                principalTable: "UserProgressions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAchievements_UserProgressions_UserProgressionId",
                table: "UserAchievements");

            migrationBuilder.DropTable(
                name: "AbuseReports");

            migrationBuilder.DropTable(
                name: "Leaderboards");

            migrationBuilder.DropTable(
                name: "MarketplaceTransactions");

            migrationBuilder.DropTable(
                name: "SimulationScenarios");

            migrationBuilder.DropTable(
                name: "TeamLeaderboards");

            migrationBuilder.DropTable(
                name: "UserInventories");

            migrationBuilder.DropTable(
                name: "UserMilestoneProgresses");

            migrationBuilder.DropTable(
                name: "UserSeasonalEventProgresses");

            migrationBuilder.DropTable(
                name: "UserStreaks");

            migrationBuilder.DropTable(
                name: "ProjectSimulations");

            migrationBuilder.DropTable(
                name: "MarketplaceItems");

            migrationBuilder.DropTable(
                name: "Milestones");

            migrationBuilder.DropTable(
                name: "SeasonalEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAchievements_UserProgressions_UserProgressionId",
                table: "UserAchievements",
                column: "UserProgressionId",
                principalTable: "UserProgressions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
