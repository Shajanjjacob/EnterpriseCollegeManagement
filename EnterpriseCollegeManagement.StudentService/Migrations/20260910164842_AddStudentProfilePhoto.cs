using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCollegeManagement.StudentService.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoUrl",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhotoUrl",
                table: "Students");
        }
    }
}
