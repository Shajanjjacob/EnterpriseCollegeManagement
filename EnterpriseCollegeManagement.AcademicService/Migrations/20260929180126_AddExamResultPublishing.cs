using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseCollegeManagement.AcademicService.Migrations
{
    /// <inheritdoc />
    public partial class AddExamResultPublishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResultPublished",
                table: "ExamAttendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResultPublishedBy",
                table: "ExamAttendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResultPublishedDate",
                table: "ExamAttendances",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResultPublished",
                table: "ExamAttendances");

            migrationBuilder.DropColumn(
                name: "ResultPublishedBy",
                table: "ExamAttendances");

            migrationBuilder.DropColumn(
                name: "ResultPublishedDate",
                table: "ExamAttendances");
        }
    }
}
