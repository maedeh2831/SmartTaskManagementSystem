using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class MakeProjectKeyIndexFiltered : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_WorkspaceId_Key",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_WorkspaceId_Key",
                table: "Projects",
                columns: new[] { "WorkspaceId", "Key" },
                unique: true,
                filter: "[ViewState] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_WorkspaceId_Key",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_WorkspaceId_Key",
                table: "Projects",
                columns: new[] { "WorkspaceId", "Key" },
                unique: true);
        }
    }
}
