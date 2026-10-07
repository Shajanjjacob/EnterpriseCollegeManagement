using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCollegeManagement.TeacherService.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoUrl",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhotoUrl",
                table: "Teachers");
        }
    }
}
