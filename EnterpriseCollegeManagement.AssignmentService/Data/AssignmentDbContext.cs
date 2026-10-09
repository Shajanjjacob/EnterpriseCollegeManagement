using EnterpriseCollegeManagement.AssignmentService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AssignmentService.Data
{
    public class AssignmentDbContext : DbContext
    {
        public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options) : base(options)
        {

        }

        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
    }
}
