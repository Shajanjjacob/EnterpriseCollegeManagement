namespace EnterpriseCollegeManagement.TeacherService.DTOs.Requests
{
    public class UpdateTeacherProfileRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
