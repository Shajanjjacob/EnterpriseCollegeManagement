namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Responses
{
    public class AssignmentSubmissionResponseDto
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public string StudentUserId { get; set; } = string.Empty;

        public string? SubmissionText { get; set; }

        public string? FileUrl { get; set; }

        public DateTime SubmittedAt { get; set; }

        public decimal? MarksObtained { get; set; }

        public string? Feedback { get; set; }

        public DateTime? GradedAt { get; set; }
    }
}
