/*
| Migration  : Phase3_Marketplace
| Date       : 2026-08-29
| Purpose    : اضافه کردن جداول بازار (Marketplace, Inventory, Transactions)
*/

using Microsoft.EntityFrameworkCore.Migrations;

namespace SmartTask.Web.Migrations
{
    public partial class Phase3_Marketplace : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create MarketplaceItem table
            migrationBuilder.CreateTable(
                name: "MarketplaceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    TotalSold = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsLimitedTime = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvailableUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ViewState = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceItems", x => x.Id);
                });

            // Create UserInventory table
            migrationBuilder.CreateTable(
                name: "UserInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MarketplaceItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsEquipped = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AcquiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EquippedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewState = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInventories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInventories_MarketplaceItems_MarketplaceItemId",
                        column: x => x.MarketplaceItemId,
                        principalTable: "MarketplaceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create MarketplaceTransaction table
            migrationBuilder.CreateTable(
                name: "MarketplaceTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserWalletId = table.Column<int>(type: "int", nullable: false),
                    MarketplaceItemId = table.Column<int>(type: "int", nullable: false),
                    PointsSpent = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViewState = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_MarketplaceItems_MarketplaceItemId",
                        column: x => x.MarketplaceItemId,
                        principalTable: "MarketplaceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceTransactions_UserWallets_UserWalletId",
                        column: x => x.UserWalletId,
                        principalTable: "UserWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItems_Category",
                table: "MarketplaceItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItems_IsActive",
                table: "MarketplaceItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItems_LimitedTime",
                table: "MarketplaceItems",
                columns: new[] { "IsLimitedTime", "AvailableFrom", "AvailableUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_UserId_MarketplaceItemId",
                table: "UserInventories",
                columns: new[] { "UserId", "MarketplaceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_UserId",
                table: "UserInventories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_IsEquipped",
                table: "UserInventories",
                column: "IsEquipped");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_UserId",
                table: "MarketplaceTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_UserWalletId",
                table: "MarketplaceTransactions",
                column: "UserWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_Status",
                table: "MarketplaceTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_TransactionDate",
                table: "MarketplaceTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceTransactions_UserIdDate",
                table: "MarketplaceTransactions",
                columns: new[] { "UserId", "TransactionDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceTransactions");

            migrationBuilder.DropTable(
                name: "UserInventories");

            migrationBuilder.DropTable(
                name: "MarketplaceItems");
        }
    }
}
