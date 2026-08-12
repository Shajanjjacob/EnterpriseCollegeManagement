namespace EnterpriseCollegeManagement.IdentityService.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }

        public string? UserId { get; set; }  // who change 

        public string Actiom { get; set; } = string.Empty; //what action performed 

        public string EntityName { get; set; } = string.Empty; // which table get affected 
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }

      
        public string? NewValues { get; set; }

        
        public string? Description { get; set; }

        
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

       
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
