namespace EnterpriseCollegeManagement.AcademicService.DTOs.Requests
{
    public class CreateStudentAnswerRequestDto
    {
        public int ExamAttendanceId { get; set; }

        public int QuestionId { get; set; }

        public string SelectedOption { get; set; } = string.Empty;
    }
}
