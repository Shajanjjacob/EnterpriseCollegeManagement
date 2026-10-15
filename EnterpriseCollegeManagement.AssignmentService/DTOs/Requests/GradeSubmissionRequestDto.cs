using System.ComponentModel.DataAnnotations;

namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Requests
{
    public class GradeSubmissionRequestDto
    {
        [Required]
        [Range(typeof(decimal), "0", "1000000")]
        public decimal MarksObtained { get; set; }

        [StringLength(2000)]
        public string? Feedback { get; set; }
    }
}
