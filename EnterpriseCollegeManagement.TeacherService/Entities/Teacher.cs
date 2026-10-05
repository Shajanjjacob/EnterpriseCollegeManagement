namespace EnterpriseCollegeManagement.TeacherService.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string EmployeeCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

       
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        
        public bool IsDeleted { get; set; }

        public string? DeletedBy { get; set; }

        public DateTime? DeletedDate { get; set; }
    }
}
