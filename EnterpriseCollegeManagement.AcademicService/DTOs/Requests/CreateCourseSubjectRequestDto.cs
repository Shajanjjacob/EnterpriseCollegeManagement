namespace EnterpriseCollegeManagement.AcademicService.DTOs.Requests
{
    public class CreateCourseSubjectRequestDto
    {
        public int CourseId { get; set; }
        public int SubjectId { get; set; }
        public int Semester { get; set; }
    } 
}
