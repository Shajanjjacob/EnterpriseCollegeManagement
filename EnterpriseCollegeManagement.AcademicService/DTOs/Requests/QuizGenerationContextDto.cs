namespace EnterpriseCollegeManagement.AcademicService.DTOs.Requests
{
    public class QuizGenerationContextDto
    {
        //internal context for AI req

        public int ExamId { get; set; }

        public string ExamTitle { get; set; } = string.Empty;

        public string? ExamDescription { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public int Semester { get; set; }

        public string Topic { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public int NumberOfQuestions { get; set; }

        public int MarksPerQuestion { get; set; }
    }
}
