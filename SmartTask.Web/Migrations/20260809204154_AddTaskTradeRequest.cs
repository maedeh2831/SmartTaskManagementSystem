using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTradeRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskTradeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RequesterUserId = table.Column<int>(type: "int", nullable: false),
                    TargetUserId = table.Column<int>(type: "int", nullable: false),
                    RequesterTaskId = table.Column<int>(type: "int", nullable: false),
                    TargetTaskId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTradeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskTradeRequests_ApplicationUsers_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskTradeRequests_ApplicationUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskTradeRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTradeRequests_TaskItems_RequesterTaskId",
                        column: x => x.RequesterTaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskTradeRequests_TaskItems_TargetTaskId",
                        column: x => x.TargetTaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskTradeRequests_ProjectId",
                table: "TaskTradeRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTradeRequests_RequesterTaskId",
                table: "TaskTradeRequests",
                column: "RequesterTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTradeRequests_RequesterUserId",
                table: "TaskTradeRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTradeRequests_TargetTaskId",
                table: "TaskTradeRequests",
                column: "TargetTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTradeRequests_TargetUserId",
                table: "TaskTradeRequests",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskTradeRequests");
        }
    }
}
