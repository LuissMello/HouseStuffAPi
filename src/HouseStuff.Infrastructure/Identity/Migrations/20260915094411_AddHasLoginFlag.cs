using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddHasLoginFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasLogin",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasLogin",
                table: "AspNetUsers");
        }
    }
}
