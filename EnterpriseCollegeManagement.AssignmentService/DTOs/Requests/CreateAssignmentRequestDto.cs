using System.ComponentModel.DataAnnotations;

namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Requests
{
    public class CreateAssignmentRequestDto
    {
        [Required]
        [StringLength(maximumLength: 2000, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(1,int.MaxValue)]
        public int CourseSubjectId { get; set; }
        [Required]
        public DateTime DueDate { get; set; }

        [Range(typeof(decimal), "0.01", "1000000")]
        public decimal MaxMarks { get; set; }
        public string? TeacherUserId { get; set; }  //while admin assign assignment for the teacher
    }
}
