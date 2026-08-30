using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAllGamificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductivityMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    ProductivityScore = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    TaskCompletionRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    OnTimeDeliveryRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    ConsistencyRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    QualityScore = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    TotalTasksAssigned = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalTasksCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OnTimeTasksCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OverdueTasksCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TasksReopened = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    WorkedDaysThisPeriod = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalDaysInPeriod = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LongestStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentTier = table.Column<int>(type: "int", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrentPeriod = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductivityMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductivityMetrics_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductivityMetrics_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductivityScoreHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductivityMetricsId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductivityScore = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    TaskCompletionRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    OnTimeDeliveryRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    ConsistencyRate = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    QualityScore = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    TasksCompletedThisPeriod = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OnTimeTasksThisPeriod = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Daily"),
                    TierAtSnapshot = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductivityScoreHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductivityScoreHistories_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductivityScoreHistories_ProductivityMetrics_ProductivityMetricsId",
                        column: x => x.ProductivityMetricsId,
                        principalTable: "ProductivityMetrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductivityMetrics_UserId_WorkspaceId_IsCurrentPeriod",
                table: "ProductivityMetrics",
                columns: new[] { "UserId", "WorkspaceId", "IsCurrentPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductivityMetrics_WorkspaceId",
                table: "ProductivityMetrics",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductivityScoreHistories_ProductivityMetricsId",
                table: "ProductivityScoreHistories",
                column: "ProductivityMetricsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductivityScoreHistories_UserId_SnapshotDate",
                table: "ProductivityScoreHistories",
                columns: new[] { "UserId", "SnapshotDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductivityScoreHistories");

            migrationBuilder.DropTable(
                name: "ProductivityMetrics");
        }
    }
}
