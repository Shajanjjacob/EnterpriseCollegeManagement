namespace EnterpriseCollegeManagement.AcademicService.DTOs.Requests
{
    public class CreateCourseRequest
    {
        public string Code {  get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int DurationYears { get; set; }

        public int DepartmentId { get; set; }
    }
}
