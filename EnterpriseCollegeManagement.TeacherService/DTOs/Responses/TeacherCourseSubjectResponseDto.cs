namespace EnterpriseCollegeManagement.TeacherService.DTOs.Responses
{
    public class TeacherCourseSubjectResponseDto
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }

        public int CourseSubjectId { get; set; }

        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public int SubjectId { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public int Semester { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; }
    }
}
