namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class Course
    {
        public int Id {  get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int DurationYears { get; set; }


        public ICollection<CourseSubject> CourseSubjects {  get; set; } = new List<CourseSubject>();  //many to many with junction table 

        //public ICollection<Subject> Subjects { get; set; } = new List<Subject>();  same but done using junction table explicit


      
        public int DepartmentId { get; set; }   //ref from student service 


        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

       
        public bool IsDeleted { get; set; }

        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
