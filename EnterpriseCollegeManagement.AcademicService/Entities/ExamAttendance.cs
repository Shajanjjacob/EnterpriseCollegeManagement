namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class ExamAttendance
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;
        public string StudentUserId { get; set; } //identity userid 
        public DateTime StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public int Score { get; set; }

        public int TotalMarks { get; set; }

        public bool IsSubmitted { get; set; }

        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>(); //list of student xam ans records 
    }
}
