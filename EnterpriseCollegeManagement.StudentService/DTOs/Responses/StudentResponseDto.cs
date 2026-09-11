namespace EnterpriseCollegeManagement.StudentService.DTOs.Responses
{
    public class StudentResponseDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string AdmissionNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string? ProfilePhotoUrl { get; set; }


        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }
    }
}
