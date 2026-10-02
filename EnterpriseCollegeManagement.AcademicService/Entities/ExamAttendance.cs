namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class ExamAttendance
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;
        public string StudentUserId { get; set; } //identity userid 
        public DateTime StartedAt { get; set; }

        public DateTime ExpiresAt { get; set; }  //for autosubmit answer based on exam durations

        public DateTime? SubmittedAt { get; set; }



        public int Score { get; set; }

        public int TotalMarks { get; set; }

        public bool IsSubmitted { get; set; }

        //result based 
        public bool IsResultPublished { get; set; }

        public string? ResultPublishedBy { get; set; }

        public DateTime? ResultPublishedDate { get; set; }



        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>(); //list of student xam ans records 
    }
}
