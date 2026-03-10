using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerRookies.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleSpeakers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Speakers table
            migrationBuilder.CreateTable(
                name: "Speakers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Speakers", x => x.Id);
                });

            // 2. Create WorkshopSpeakers join table
            migrationBuilder.CreateTable(
                name: "WorkshopSpeakers",
                columns: table => new
                {
                    WorkshopId = table.Column<int>(type: "int", nullable: false),
                    SpeakerId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopSpeakers", x => new { x.WorkshopId, x.SpeakerId });
                    table.ForeignKey(
                        name: "FK_WorkshopSpeakers_Speakers_SpeakerId",
                        column: x => x.SpeakerId,
                        principalTable: "Speakers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkshopSpeakers_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Speakers_Name",
                table: "Speakers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopSpeakers_SpeakerId",
                table: "WorkshopSpeakers",
                column: "SpeakerId");

            // 3. Migrate existing speaker data from Workshops to Speakers table
            migrationBuilder.Sql(@"
                INSERT INTO Speakers (Name, Description, ImagePath, CreatedAt, UpdatedAt)
                SELECT DISTINCT w.SpeakerName, w.SpeakerDescription, w.SpeakerImagePath, GETUTCDATE(), GETUTCDATE()
                FROM Workshops w
                WHERE w.SpeakerName IS NOT NULL AND w.SpeakerName <> '';

                INSERT INTO WorkshopSpeakers (WorkshopId, SpeakerId, SortOrder)
                SELECT w.Id, s.Id, 0
                FROM Workshops w
                INNER JOIN Speakers s ON s.Name = w.SpeakerName
                    AND s.Description = w.SpeakerDescription
                WHERE w.SpeakerName IS NOT NULL AND w.SpeakerName <> '';
            ");

            // 4. Now safe to drop old columns
            migrationBuilder.DropColumn(
                name: "SpeakerDescription",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "SpeakerImagePath",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "SpeakerName",
                table: "Workshops");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add old columns
            migrationBuilder.AddColumn<string>(
                name: "SpeakerName",
                table: "Workshops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpeakerDescription",
                table: "Workshops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpeakerImagePath",
                table: "Workshops",
                type: "nvarchar(max)",
                nullable: true);

            // Migrate data back
            migrationBuilder.Sql(@"
                UPDATE w
                SET w.SpeakerName = s.Name,
                    w.SpeakerDescription = s.Description,
                    w.SpeakerImagePath = s.ImagePath
                FROM Workshops w
                INNER JOIN WorkshopSpeakers ws ON ws.WorkshopId = w.Id AND ws.SortOrder = 0
                INNER JOIN Speakers s ON s.Id = ws.SpeakerId;
            ");

            migrationBuilder.DropTable(
                name: "WorkshopSpeakers");

            migrationBuilder.DropTable(
                name: "Speakers");
        }
    }
}
