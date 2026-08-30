/*
| Migration  : Phase4_Leaderboards
| Date       : 2026-08-29
| Purpose    : Add Leaderboard and TeamLeaderboard entities for Phase 4
*/

using Microsoft.EntityFrameworkCore.Migrations;

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_Leaderboards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    TasksCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ProjectsCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AchievementsUnlocked = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ConsecutiveCompletionDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WeeklyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MonthlyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WeeklyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthlyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RankChangeFromPrevious = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaderboards_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leaderboards_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    TasksCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ProjectsCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TeamMemberCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AchievementsUnlocked = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WeeklyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MonthlyPoints = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WeeklyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthlyPointsResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AverageCompletionRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    AverageProductivity = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    ActiveMembersThisWeek = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RankChangeFromPrevious = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamLeaderboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamLeaderboards_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamLeaderboards_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leaderboards");

            migrationBuilder.DropTable(
                name: "TeamLeaderboards");
        }
    }
}
