namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!; //
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;

        public string OptionB { get; set; } = string.Empty;

        public string OptionC { get; set; } = string.Empty;

        public string OptionD { get; set; } = string.Empty;

        public string CorrectOption { get; set; } = string.Empty;

        public int Marks { get; set; }

        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>(); //one qustn can have many ans


    }
}
