namespace EnterpriseCollegeManagement.AcademicService.DTOs.Requests
{
    public class GenerateQuizRequestDto
    {
        //AI

        public int ExamId {  get; set; }
        public string Topic {  get; set; } = string.Empty;
        public string? Notes { get; set; }

        public int NumberOfQuestions { get; set; }
    }
}
