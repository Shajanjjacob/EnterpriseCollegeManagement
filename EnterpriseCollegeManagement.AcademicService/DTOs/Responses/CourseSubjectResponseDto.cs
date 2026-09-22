namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class CourseSubjectResponseDto
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;
        public int Semester { get; set; }
    }
}
