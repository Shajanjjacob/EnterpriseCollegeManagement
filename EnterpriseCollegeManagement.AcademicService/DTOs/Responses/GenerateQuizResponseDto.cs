namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class GenerateQuizResponseDto
    {
        public int ExamId { get; set; }

        public List<GeneratedQuestionDto> Questions {  get; set; } = new List<GeneratedQuestionDto>();
    }

    public class GeneratedQuestionDto
    {
        public string QuestionText { get; set; } = string.Empty;

        public string OptionA { get; set; } = string.Empty;

        public string OptionB { get; set; } = string.Empty;

        public string OptionC { get; set; } = string.Empty;

        public string OptionD { get; set; } = string.Empty;

        public string CorrectOption { get; set; } = string.Empty;

        public int Marks { get; set; }
    }
}
