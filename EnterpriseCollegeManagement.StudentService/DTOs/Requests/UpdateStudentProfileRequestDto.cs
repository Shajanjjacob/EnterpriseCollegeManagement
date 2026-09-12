using System.Drawing.Printing;

namespace EnterpriseCollegeManagement.StudentService.DTOs.Requests
{
    public class UpdateStudentProfileRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Address {  get; set; } = string.Empty;
    }
}
