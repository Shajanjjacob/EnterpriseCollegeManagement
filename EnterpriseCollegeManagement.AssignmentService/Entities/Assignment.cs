namespace EnterpriseCollegeManagement.AssignmentService.Entities
{
    public class Assignment
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CourseSubjectId { get; set; }

        public string TeacherUserId { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public decimal MaxMarks { get; set; }

        public bool IsPublished { get; set; } = false;

        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

      
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public string? DeletedBy { get; set; }

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();  //1 assignment many submit
    }
}
