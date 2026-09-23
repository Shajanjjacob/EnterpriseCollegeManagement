namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class StudentAnswer
    {
        public int Id { get; set; }

        public int ExamAttendanceId { get; set; } //xam attended record 

        public ExamAttendance ExamAttendance { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public string SelectedOption { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        public int MarksAwarded { get; set; }
    }
}
