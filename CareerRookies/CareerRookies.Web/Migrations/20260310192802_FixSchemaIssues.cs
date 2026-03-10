using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerRookies.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixSchemaIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add Status column with default 'Pending' BEFORE dropping IsApproved
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Articles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            // 2. Migrate existing data: IsApproved=true -> 'Approved', false -> 'Pending'
            migrationBuilder.Sql("UPDATE Articles SET Status = 'Approved' WHERE IsApproved = 1");
            migrationBuilder.Sql("UPDATE Articles SET Status = 'Pending' WHERE IsApproved = 0");

            // 3. Drop the old IsApproved column
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Articles");

            // 4. Add StudentClassId column with default value 1 (first seeded class)
            migrationBuilder.AddColumn<int>(
                name: "StudentClassId",
                table: "WorkshopRegistrations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // 5. Try to map existing StudentClass string values to StudentClassId
            migrationBuilder.Sql(@"
                UPDATE wr SET wr.StudentClassId = sc.Id
                FROM WorkshopRegistrations wr
                INNER JOIN StudentClasses sc ON sc.Name = wr.StudentClass
                WHERE wr.StudentClass IS NOT NULL AND wr.StudentClass <> ''");

            // 6. Drop the old StudentClass string column
            migrationBuilder.DropColumn(
                name: "StudentClass",
                table: "WorkshopRegistrations");

            // 7. Tighten enum string column sizes
            migrationBuilder.AlterColumn<string>(
                name: "MediaType",
                table: "WorkshopMedia",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorType",
                table: "Testimonials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "CareerResources",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // 8. Create FK index and constraint
            migrationBuilder.CreateIndex(
                name: "IX_WorkshopRegistrations_StudentClassId",
                table: "WorkshopRegistrations",
                column: "StudentClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkshopRegistrations_StudentClasses_StudentClassId",
                table: "WorkshopRegistrations",
                column: "StudentClassId",
                principalTable: "StudentClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkshopRegistrations_StudentClasses_StudentClassId",
                table: "WorkshopRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopRegistrations_StudentClassId",
                table: "WorkshopRegistrations");

            // Restore StudentClass string column
            migrationBuilder.AddColumn<string>(
                name: "StudentClass",
                table: "WorkshopRegistrations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Map StudentClassId back to name
            migrationBuilder.Sql(@"
                UPDATE wr SET wr.StudentClass = sc.Name
                FROM WorkshopRegistrations wr
                INNER JOIN StudentClasses sc ON sc.Id = wr.StudentClassId");

            migrationBuilder.DropColumn(
                name: "StudentClassId",
                table: "WorkshopRegistrations");

            // Restore IsApproved column
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Map Status back to IsApproved
            migrationBuilder.Sql("UPDATE Articles SET IsApproved = 1 WHERE Status = 'Approved'");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Articles");

            migrationBuilder.AlterColumn<string>(
                name: "MediaType",
                table: "WorkshopMedia",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorType",
                table: "Testimonials",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "CareerResources",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
