namespace EnterpriseCollegeManagement.TeacherService.DTOs.Requests
{
    public class CreateTeacherProfileRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
