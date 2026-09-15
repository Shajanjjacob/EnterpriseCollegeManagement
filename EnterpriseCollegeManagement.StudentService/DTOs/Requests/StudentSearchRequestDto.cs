namespace EnterpriseCollegeManagement.StudentService.DTOs.Requests
{
    public class StudentSearchRequestDto
    {
        public string? search { get; set; }

        public int? StudentId { get; set; }

        public string? AdmissionNumber { get; set; }

        public int? DepartmentId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
