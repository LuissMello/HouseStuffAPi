using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861 // Código gerado pelo Entity Framework usa arrays nos índices compostos.

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseholdTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Pots_Id_ResidenceId",
                table: "Pots",
                columns: new[] { "Id", "ResidenceId" });

            migrationBuilder.CreateTable(
                name: "HouseholdTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecurrenceDays = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HouseholdTasks_Pots_PotId_ResidenceId",
                        columns: x => new { x.PotId, x.ResidenceId },
                        principalTable: "Pots",
                        principalColumns: new[] { "Id", "ResidenceId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTasks_PotId_ResidenceId",
                table: "HouseholdTasks",
                columns: new[] { "PotId", "ResidenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTasks_ResidenceId_PotId_IsActive",
                table: "HouseholdTasks",
                columns: new[] { "ResidenceId", "PotId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTasks_ResidenceId_PotId_NormalizedName",
                table: "HouseholdTasks",
                columns: new[] { "ResidenceId", "PotId", "NormalizedName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HouseholdTasks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Pots_Id_ResidenceId",
                table: "Pots");
        }
    }
}
#pragma warning restore CA1861
