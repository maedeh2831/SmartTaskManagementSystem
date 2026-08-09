using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddOverdueCascadeLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OverdueCascadeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceTaskId = table.Column<int>(type: "int", nullable: false),
                    ImpactedTaskId = table.Column<int>(type: "int", nullable: false),
                    DelayDaysApplied = table.Column<int>(type: "int", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdueCascadeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverdueCascadeLogs_TaskItems_ImpactedTaskId",
                        column: x => x.ImpactedTaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OverdueCascadeLogs_TaskItems_SourceTaskId",
                        column: x => x.SourceTaskId,
                        principalTable: "TaskItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OverdueCascadeLogs_ImpactedTaskId",
                table: "OverdueCascadeLogs",
                column: "ImpactedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_OverdueCascadeLogs_SourceTaskId_ImpactedTaskId",
                table: "OverdueCascadeLogs",
                columns: new[] { "SourceTaskId", "ImpactedTaskId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OverdueCascadeLogs");
        }
    }
}
