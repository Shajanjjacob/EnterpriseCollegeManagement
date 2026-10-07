namespace EnterpriseCollegeManagement.TeacherService.DTOs.Responses
{
    public class TeacherResponseDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string EmployeeCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? ProfilePhotoUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
