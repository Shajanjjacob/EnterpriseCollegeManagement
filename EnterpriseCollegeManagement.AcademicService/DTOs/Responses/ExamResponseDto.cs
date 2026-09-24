namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class ExamResponseDto
    {
        public int Id { get; set; }

        public int CourseSubjectId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public int Semester { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DurationMinutes { get; set; }

        public int TotalMarks { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
