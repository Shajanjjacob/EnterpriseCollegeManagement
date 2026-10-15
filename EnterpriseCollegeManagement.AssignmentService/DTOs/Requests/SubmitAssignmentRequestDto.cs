using System.ComponentModel.DataAnnotations;

namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Requests
{
    public class SubmitAssignmentRequestDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int AssignmentId { get; set; }

        public string? SubmissionText { get; set; }

        public IFormFile? File { get; set; }
    }
}
