namespace EnterpriseCollegeManagement.AcademicService.Entities
{
    public class Subject
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Credits { get; set; }


        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>(); //many to many with junction table

        //public ICollection<Course> courses { get; set; } = new List<Course>(); like this without junction

       //ref frm student service 
        public int DepartmentId { get; set; }

        
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

       
        public bool IsDeleted { get; set; }

        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
