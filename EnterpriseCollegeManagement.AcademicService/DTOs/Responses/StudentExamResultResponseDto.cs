namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class StudentExamResultResponseDto
    {
        public int ExamId { get; set; }

        public string ExamTitle { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public int Semester { get; set; }

        public int Score { get; set; }

        public int TotalMarks { get; set; }

        public DateTime SubmittedAt { get; set; }
    }
}
