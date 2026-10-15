namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Requests
{
    public class AssignmentReviewContextDto
    {
        public int AssignmentId { get; set; }

        public int SubmissionId { get; set; }

        public string AssignmentTitle { get; set; } = string.Empty;

        public string AssignmentDescription { get; set; } = string.Empty;

        public decimal MaxMarks { get; set; }

        public string SubmissionContent { get; set; } = string.Empty;
    }
}
