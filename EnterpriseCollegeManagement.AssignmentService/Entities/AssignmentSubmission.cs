namespace EnterpriseCollegeManagement.AssignmentService.Entities
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public Assignment Assignment { get; set; }  //key from assignment table 

        public string StudentUserId { get; set; } = string.Empty;

        public string? SubmissionText { get; set; }

        public string? FileUrl { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public decimal? MarksObtained { get; set; }

        public string? Feedback { get; set; }

        public DateTime? GradedAt { get; set; }

      
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

      
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }

    }
}
