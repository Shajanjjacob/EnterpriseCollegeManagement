namespace EnterpriseCollegeManagement.StudentService.Entities
{
    public class Department
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public ICollection<Student> students { get; set; } = new List<Student>();
    }
}
