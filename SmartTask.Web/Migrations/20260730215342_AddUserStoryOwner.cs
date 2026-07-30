using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStoryOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "UserStories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStories_OwnerId",
                table: "UserStories",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStories_ApplicationUsers_OwnerId",
                table: "UserStories",
                column: "OwnerId",
                principalTable: "ApplicationUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStories_ApplicationUsers_OwnerId",
                table: "UserStories");

            migrationBuilder.DropIndex(
                name: "IX_UserStories_OwnerId",
                table: "UserStories");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "UserStories");
        }
    }
}
