using Microsoft.EntityFrameworkCore.Migrations;

namespace SmartTask.Web.Migrations
{
    public partial class Phase5_ProjectSimulation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create ProjectSimulation table
            migrationBuilder.CreateTable(
                name: "ProjectSimulations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    BaselineStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaselineEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTasksCount = table.Column<int>(type: "int", nullable: false),
                    CriticalPathCalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriticalPathLengthDays = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSimulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSimulations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create SimulationScenario table
            migrationBuilder.CreateTable(
                name: "SimulationScenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectSimulationId = table.Column<int>(type: "int", nullable: false),
                    SimulatedTaskId = table.Column<int>(type: "int", nullable: false),
                    ScenarioName = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    OriginalTaskEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DelayDays = table.Column<int>(type: "int", nullable: false),
                    NewProjectEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalProjectEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectDelayDays = table.Column<int>(type: "int", nullable: false),
                    TotalAffectedTasks = table.Column<int>(type: "int", nullable: false),
                    AffectedTasksJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriticalPathJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RiskLevel = table.Column<string>(type: "nvarchar(50)", nullable: false, defaultValue: "Medium"),
                    SimulatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulationScenarios_ProjectSimulations_ProjectSimulationId",
                        column: x => x.ProjectSimulationId,
                        principalTable: "ProjectSimulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SimulationScenarios_TaskItems_SimulatedTaskId",
                        column: x => x.SimulatedTaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulations_ProjectId",
                table: "ProjectSimulations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulations_CreatedDate",
                table: "ProjectSimulations",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_ProjectSimulationId",
                table: "SimulationScenarios",
                column: "ProjectSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_SimulatedTaskId",
                table: "SimulationScenarios",
                column: "SimulatedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenarios_SimulatedAt",
                table: "SimulationScenarios",
                column: "SimulatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimulationScenarios");

            migrationBuilder.DropTable(
                name: "ProjectSimulations");
        }
    }
}
