namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Responses
{
    public class AIReviewResponseDto  //response frm ai based on assignment pdf (review)
    {

        public decimal SuggestedMarks { get; set; }

        public string Summary { get; set; } = string.Empty;

        public List<string> Strengths { get; set; } = new();

        public List<string> Improvements { get; set; } = new();

        public string SuggestedFeedback { get; set; } = string.Empty;
    }
}
