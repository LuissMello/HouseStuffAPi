using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAvailability : Migration
    {
        private static readonly string[] AvailabilityIndexColumns = ["ResidenceId", "PotId", "IsActive", "NextAvailableAt"];
        private static readonly string[] ActiveIndexColumns = ["ResidenceId", "PotId", "IsActive"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HouseholdTasks_ResidenceId_PotId_IsActive",
                table: "HouseholdTasks");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextAvailableAt",
                table: "HouseholdTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTasks_ResidenceId_PotId_IsActive_NextAvailableAt",
                table: "HouseholdTasks",
                columns: AvailabilityIndexColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HouseholdTasks_ResidenceId_PotId_IsActive_NextAvailableAt",
                table: "HouseholdTasks");

            migrationBuilder.DropColumn(
                name: "NextAvailableAt",
                table: "HouseholdTasks");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdTasks_ResidenceId_PotId_IsActive",
                table: "HouseholdTasks",
                columns: ActiveIndexColumns);
        }
    }
}
