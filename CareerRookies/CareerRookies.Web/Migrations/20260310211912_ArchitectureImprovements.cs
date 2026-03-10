using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerRookies.Web.Migrations
{
    /// <inheritdoc />
    public partial class ArchitectureImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkshopRegistrations_WorkshopId",
                table: "WorkshopRegistrations");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Workshops",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Workshops",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "Workshops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Workshops",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Testimonials",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Testimonials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Testimonials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Testimonials",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Articles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Articles",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            // Populate slugs for existing records using their Ids as temporary unique slugs
            migrationBuilder.Sql("UPDATE Workshops SET Slug = 'workshop-' + CAST(Id AS NVARCHAR(10)) WHERE Slug = ''");
            migrationBuilder.Sql("UPDATE Articles SET Slug = 'article-' + CAST(Id AS NVARCHAR(10)) WHERE Slug = ''");

            // Set existing testimonials as approved (they were all visible before)
            migrationBuilder.Sql("UPDATE Testimonials SET IsApproved = 1");
            migrationBuilder.Sql("UPDATE Testimonials SET UpdatedAt = GETUTCDATE() WHERE UpdatedAt = '0001-01-01'");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Date",
                table: "Workshops",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_IsDeleted",
                table: "Workshops",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Slug",
                table: "Workshops",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopRegistrations_WorkshopId_StudentName_StudentClassId",
                table: "WorkshopRegistrations",
                columns: new[] { "WorkshopId", "StudentName", "StudentClassId" });

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_IsApproved",
                table: "Testimonials",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_IsDeleted",
                table: "Testimonials",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsDeleted",
                table: "Articles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Slug",
                table: "Articles",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Status",
                table: "Articles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_Date",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_IsDeleted",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_Slug",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopRegistrations_WorkshopId_StudentName_StudentClassId",
                table: "WorkshopRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_Testimonials_IsApproved",
                table: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Testimonials_IsDeleted",
                table: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Articles_IsDeleted",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_Slug",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_Status",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Testimonials");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Articles");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopRegistrations_WorkshopId",
                table: "WorkshopRegistrations",
                column: "WorkshopId");
        }
    }
}
