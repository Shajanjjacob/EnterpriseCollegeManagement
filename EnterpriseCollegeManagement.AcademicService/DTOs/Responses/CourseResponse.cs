namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class CourseResponse
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DurationYears { get; set; }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }
}
