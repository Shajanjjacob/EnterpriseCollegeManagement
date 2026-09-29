namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class StudentAnswerResponseDto
    {
        public int Id { get; set; }

        public int ExamAttendanceId { get; set; }

        public int QuestionId { get; set; }

        public string SelectedOption { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public int MarksAwarded { get; set; }
    }
}
