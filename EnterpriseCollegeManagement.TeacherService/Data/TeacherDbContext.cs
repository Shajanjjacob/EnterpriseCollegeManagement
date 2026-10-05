using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.TeacherService.Data
{
    public class TeacherDbContext : DbContext
    {
        public TeacherDbContext(DbContextOptions<TeacherDbContext>options ) : base( options )
        {

        }
    }
}
