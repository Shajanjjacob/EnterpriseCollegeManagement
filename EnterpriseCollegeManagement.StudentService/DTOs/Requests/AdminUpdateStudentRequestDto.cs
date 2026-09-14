namespace EnterpriseCollegeManagement.StudentService.DTOs.Requests
{
    public class AdminUpdateStudentRequestDto
    {
        public string AdmissionNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; } = string.Empty;
        public string Address {  get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public DateTime EnrollmentDate { get; set; }
    }
}
