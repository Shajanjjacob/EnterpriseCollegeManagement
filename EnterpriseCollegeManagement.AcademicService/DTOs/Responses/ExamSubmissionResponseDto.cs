namespace EnterpriseCollegeManagement.AcademicService.DTOs.Responses
{
    public class ExamSubmissionResponseDto
    {
        public string AdmissionNumber { get; set; } = string.Empty;
        public bool ExamSubmitted { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public bool ResultPublished { get; set; }
    }
}
