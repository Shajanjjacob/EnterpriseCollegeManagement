using EnterpriseCollegeManagement.AcademicService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCollegeManagement.AcademicService.Data
{
    public class AcademicDbContext : DbContext
    {
        public AcademicDbContext(DbContextOptions<AcademicDbContext> options) : base(options)
        {

        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        public DbSet<CourseSubject> CoursesSubjects { get; set; }
    }
}
