namespace EnterpriseCollegeManagement.TeacherService.Entities
{
    public class TeacherCourseSubject
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }

        public Teacher? Teacher { get; set; }  //navigation 

        public int CourseSubjectId { get; set; }

        public DateTime AssignedDate { get; set; }

      
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

       
        public bool IsDeleted { get; set; }

        public string? DeletedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

       
       
    }
}
