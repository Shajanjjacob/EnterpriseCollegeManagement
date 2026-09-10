namespace EnterpriseCollegeManagement.StudentService.Entities
{
    public class Student
    {
        public int Id { get; set; }

        
        public string UserId { get; set; } = string.Empty; //frm identity

        public string AdmissionNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string? ProfilePhotoUrl { get; set; }

        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;
        public DateTime EnrollmentDate { get; set; }

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
