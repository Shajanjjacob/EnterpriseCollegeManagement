namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class QuestionResponseDto
    {
        public int Id { get; set; }

        public int ExamId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public string OptionA { get; set; } = string.Empty;

        public string OptionB { get; set; } = string.Empty;

        public string OptionC { get; set; } = string.Empty;

        public string OptionD { get; set; } = string.Empty;

        public string CorrectOption { get; set; } = string.Empty;

        public int Marks { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
