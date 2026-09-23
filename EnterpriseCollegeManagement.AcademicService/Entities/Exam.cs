namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class Exam
    {

        public int Id { get; set; }

        public int CourseSubjectId { get; set; }

        public CourseSubject courseSubject { get; set; } //many xam belongs to one coursesubject

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } 

        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public bool IsPublished { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsDeleted { get; set; }

        public string? DeletedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>(); // one xam can have many qustns

        public ICollection<ExamAttendance> ExamAttendances { get; set; } = new List<ExamAttendance>(); //one xam have somay xamattendance 
    }
}
