using EnterpriseCollegeManagement.TeacherService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.TeacherService.Data
{
    public class TeacherDbContext : DbContext
    {
        public TeacherDbContext(DbContextOptions<TeacherDbContext>options ) : base( options )
        {

        }

        public DbSet<Teacher> Teachers { get; set; }

        public DbSet<TeacherCourseSubject> TeacherCourseSubjects { get; set; }
    }
}
