using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddShoppingCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShoppingCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCategories", x => x.Id);
                    table.UniqueConstraint("AK_ShoppingCategories_Id_ResidenceId", x => new { x.Id, x.ResidenceId });
                    table.ForeignKey(
                        name: "FK_ShoppingCategories_Residences_ResidenceId",
                        column: x => x.ResidenceId,
                        principalTable: "Residences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingItems_ShoppingCategories_CategoryId_ResidenceId",
                        columns: x => new { x.CategoryId, x.ResidenceId },
                        principalTable: "ShoppingCategories",
                        principalColumns: new[] { "Id", "ResidenceId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCategories_ResidenceId_DisplayOrder",
                table: "ShoppingCategories",
                columns: new[] { "ResidenceId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCategories_ResidenceId_NormalizedName",
                table: "ShoppingCategories",
                columns: new[] { "ResidenceId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItems_CategoryId_ResidenceId",
                table: "ShoppingItems",
                columns: new[] { "CategoryId", "ResidenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItems_ResidenceId_CategoryId_NormalizedName",
                table: "ShoppingItems",
                columns: new[] { "ResidenceId", "CategoryId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingItems");

            migrationBuilder.DropTable(
                name: "ShoppingCategories");
        }
    }
}
