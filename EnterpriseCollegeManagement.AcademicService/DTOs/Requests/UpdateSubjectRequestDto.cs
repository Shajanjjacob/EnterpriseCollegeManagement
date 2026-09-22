namespace EnterpriseCollegeManagement.AcademicService.DTOs.Requests
{
    public class UpdateSubjectRequestDto
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int DepartmentId { get; set; }
    }
}
