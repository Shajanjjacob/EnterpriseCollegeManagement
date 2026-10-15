namespace EnterpriseCollegeManagement.AssignmentService.DTOs.Responses
{
    public class AssignmentResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CourseSubjectId { get; set; }

        public string TeacherUserId { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public decimal MaxMarks { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
