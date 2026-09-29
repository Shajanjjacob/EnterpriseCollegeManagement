namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class ExamAttendanceResponseDto
    {
        public int Id { get; set; }

        public int ExamId { get; set; }

        public string StudentUserId { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public int Score { get; set; }

        public int TotalMarks { get; set; }

        public bool IsSubmitted { get; set; }
        //result 
        public bool IsResultPublished { get; set; }

        public string? ResultPublishedBy { get; set; }

        public DateTime? ResultPublishedDate { get; set; }
    }
}
