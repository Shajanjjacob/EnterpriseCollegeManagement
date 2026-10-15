using System.ComponentModel.DataAnnotations;

namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Requests
{
    public class UpdateAssignmentRequestDto
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CourseSubjectId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Range(typeof(decimal), "0.01", "1000000")]
        public decimal MaxMarks { get; set; }

        // admin can select a teacher and change teacher
        public string? TeacherUserId { get; set; }
    }
}
