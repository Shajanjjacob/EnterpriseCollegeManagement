namespace EnterpriseCollegeManagement.StudentService.Entities
{
    public class Department
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public ICollection<Student> students { get; set; } = new List<Student>();

        //audit field
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        //delete
        public bool IsDeleted { get; set; }

        public string? DeletedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

    }
}
