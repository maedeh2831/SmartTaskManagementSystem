using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class MakeTeamAndSprintIndexesFiltered : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_WorkspaceId_Name",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_ProjectId_Name",
                table: "Sprints");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_WorkspaceId_Name",
                table: "Teams",
                columns: new[] { "WorkspaceId", "Name" },
                unique: true,
                filter: "[ViewState] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_ProjectId_Name",
                table: "Sprints",
                columns: new[] { "ProjectId", "Name" },
                unique: true,
                filter: "[ViewState] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_WorkspaceId_Name",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_ProjectId_Name",
                table: "Sprints");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_WorkspaceId_Name",
                table: "Teams",
                columns: new[] { "WorkspaceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_ProjectId_Name",
                table: "Sprints",
                columns: new[] { "ProjectId", "Name" },
                unique: true);
        }
    }
}
