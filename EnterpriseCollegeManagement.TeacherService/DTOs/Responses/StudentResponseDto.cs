namespace EnterpriseCollegeManagement.TeacherService.DTOs.Responses
{
    public class StudentResponseDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string AdmissionNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public int CourseId { get; set; }

        public int Semester { get; set; }

        public string Batch { get; set; } = string.Empty;
    }
}
