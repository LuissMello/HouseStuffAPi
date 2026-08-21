using System;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1861 // Código gerado pelo Entity Framework usa arrays nos índices e chaves compostos.
#nullable disable

namespace HouseStuff.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AppliesToAll = table.Column<bool>(type: "boolean", nullable: false),
                    AllDayDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.UniqueConstraint("AK_CalendarEvents_Id_ResidenceId", x => new { x.Id, x.ResidenceId });
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Residences_ResidenceId",
                        column: x => x.ResidenceId,
                        principalTable: "Residences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventParticipants",
                columns: table => new
                {
                    CalendarEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ResidenceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventParticipants", x => new { x.CalendarEventId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CalendarEventParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEventParticipants_CalendarEvents_CalendarEventId_Re~",
                        columns: x => new { x.CalendarEventId, x.ResidenceId },
                        principalTable: "CalendarEvents",
                        principalColumns: new[] { "Id", "ResidenceId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEventParticipants_CalendarEventId_ResidenceId",
                table: "CalendarEventParticipants",
                columns: new[] { "CalendarEventId", "ResidenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEventParticipants_ResidenceId_UserId",
                table: "CalendarEventParticipants",
                columns: new[] { "ResidenceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEventParticipants_UserId",
                table: "CalendarEventParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_ResidenceId_AllDayDate",
                table: "CalendarEvents",
                columns: new[] { "ResidenceId", "AllDayDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_ResidenceId_StartsAt",
                table: "CalendarEvents",
                columns: new[] { "ResidenceId", "StartsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEventParticipants");

            migrationBuilder.DropTable(
                name: "CalendarEvents");
        }
    }
}
#pragma warning restore CA1861
