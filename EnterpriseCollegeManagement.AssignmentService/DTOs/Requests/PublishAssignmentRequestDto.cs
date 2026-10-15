using System.ComponentModel.DataAnnotations;

namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Requests
{
    public class PublishAssignmentRequestDto
    {
        [Required]
        public bool IsPublished { get; set; }
    }
}
