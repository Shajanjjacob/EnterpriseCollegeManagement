namespace EnterpriseCollegeManagement.StudentService.DTOs.Responses
{
    public class CourseResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public int DurationYears { get; set; }
    }
}
