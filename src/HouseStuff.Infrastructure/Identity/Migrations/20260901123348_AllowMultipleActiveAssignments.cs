using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleActiveAssignments : Migration
    {
        private static readonly string[] ActiveAssignmentLookupColumns = ["AssignedToUserId", "CompletedAt"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_AssignedToUserId",
                table: "TaskAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_AssignedToUserId_CompletedAt",
                table: "TaskAssignments",
                columns: ActiveAssignmentLookupColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskAssignments_AssignedToUserId_CompletedAt",
                table: "TaskAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_AssignedToUserId",
                table: "TaskAssignments",
                column: "AssignedToUserId",
                unique: true,
                filter: "\"CompletedAt\" IS NULL");
        }
    }
}
