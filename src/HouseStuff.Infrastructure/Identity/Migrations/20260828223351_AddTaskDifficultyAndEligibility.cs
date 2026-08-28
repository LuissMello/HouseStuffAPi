using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDifficultyAndEligibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "HouseholdTasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Medium");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableToAllResidents",
                table: "HouseholdTasks",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_HouseholdTasks_Id_ResidenceId",
                table: "HouseholdTasks",
                columns: new[] { "Id", "ResidenceId" });

            migrationBuilder.CreateTable(
                name: "HouseholdTaskEligibleUsers",
                columns: table => new
                {
                    HouseholdTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdTaskEligibleUsers", x => new { x.HouseholdTaskId, x.UserId });
                    table.ForeignKey(
                        name: "FK_HouseholdTaskEligibleUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseholdTaskEligibleUsers_HouseholdTasks_HouseholdTaskId_R~",
                        columns: x => new { x.HouseholdTaskId, x.ResidenceId },
                        principalTable: "HouseholdTasks",
                        principalColumns: new[] { "Id", "ResidenceId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTaskEligibleUsers_HouseholdTaskId_ResidenceId",
                table: "HouseholdTaskEligibleUsers",
                columns: new[] { "HouseholdTaskId", "ResidenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTaskEligibleUsers_ResidenceId_UserId",
                table: "HouseholdTaskEligibleUsers",
                columns: new[] { "ResidenceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTaskEligibleUsers_UserId",
                table: "HouseholdTaskEligibleUsers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HouseholdTaskEligibleUsers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_HouseholdTasks_Id_ResidenceId",
                table: "HouseholdTasks");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "HouseholdTasks");

            migrationBuilder.DropColumn(
                name: "IsAvailableToAllResidents",
                table: "HouseholdTasks");
        }
    }
}
