using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettingsAndNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DateFormat",
                table: "ApplicationUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultWorkspaceId",
                table: "ApplicationUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowFullName",
                table: "ApplicationUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TaskDensity",
                table: "ApplicationUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_DefaultWorkspaceId",
                table: "ApplicationUsers",
                column: "DefaultWorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_ApplicationUserId_NotificationType",
                table: "UserNotificationPreferences",
                columns: new[] { "ApplicationUserId", "NotificationType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUsers_Workspaces_DefaultWorkspaceId",
                table: "ApplicationUsers",
                column: "DefaultWorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUsers_Workspaces_DefaultWorkspaceId",
                table: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationUsers_DefaultWorkspaceId",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "DateFormat",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "DefaultWorkspaceId",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "ShowFullName",
                table: "ApplicationUsers");

            migrationBuilder.DropColumn(
                name: "TaskDensity",
                table: "ApplicationUsers");
        }
    }
}
