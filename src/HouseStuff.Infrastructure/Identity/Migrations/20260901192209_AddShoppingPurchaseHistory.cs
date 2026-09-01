using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddShoppingPurchaseHistory : Migration
    {
        private static readonly string[] PurchaseKeyColumns = ["Id", "ResidenceId"];
        private static readonly string[] PurchaseItemLookupColumns = ["PurchaseId", "ResidenceId"];
        private static readonly string[] PurchaseHistoryLookupColumns = ["ResidenceId", "CompletedAt"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShoppingPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingPurchases", x => x.Id);
                    table.UniqueConstraint("AK_ShoppingPurchases_Id_ResidenceId", x => new { x.Id, x.ResidenceId });
                    table.ForeignKey(
                        name: "FK_ShoppingPurchases_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingPurchases_Residences_ResidenceId",
                        column: x => x.ResidenceId,
                        principalTable: "Residences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingPurchaseItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingPurchaseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingPurchaseItems_ShoppingPurchases_PurchaseId_Residenc~",
                        columns: x => new { x.PurchaseId, x.ResidenceId },
                        principalTable: "ShoppingPurchases",
                        principalColumns: PurchaseKeyColumns,
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingPurchaseItems_PurchaseId_ResidenceId",
                table: "ShoppingPurchaseItems",
                columns: PurchaseItemLookupColumns);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingPurchases_CompletedByUserId",
                table: "ShoppingPurchases",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingPurchases_ResidenceId_CompletedAt",
                table: "ShoppingPurchases",
                columns: PurchaseHistoryLookupColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingPurchaseItems");

            migrationBuilder.DropTable(
                name: "ShoppingPurchases");
        }
    }
}
