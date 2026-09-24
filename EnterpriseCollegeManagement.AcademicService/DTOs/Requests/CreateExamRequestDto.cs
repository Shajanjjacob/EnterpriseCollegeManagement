namespace EnterpriseCollegeManagement.AcademicService.DTOs.Requests
{
    public class CreateExamRequestDto
    {
        public int CourseSubjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DurationMinutes { get; set; }

        public int TotalMarks { get; set; }
    }
}
