using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workspaces_Slug",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Workspaces");

            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "Workspaces",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "DefaultLanguage",
                table: "Workspaces",
                newName: "Visibility");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Workspaces",
                newName: "CreateDate");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Workspaces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_OwnerId",
                table: "Workspaces",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workspaces_ApplicationUsers_OwnerId",
                table: "Workspaces",
                column: "OwnerId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workspaces_ApplicationUsers_OwnerId",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_OwnerId",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Workspaces");

            migrationBuilder.RenameColumn(
                name: "Visibility",
                table: "Workspaces",
                newName: "DefaultLanguage");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Workspaces",
                newName: "IsPublic");

            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "Workspaces",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Workspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Workspaces",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Workspaces",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Slug",
                table: "Workspaces",
                column: "Slug",
                unique: true);
        }
    }
}
