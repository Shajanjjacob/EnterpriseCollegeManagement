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

        //exam releated 
        public DbSet<Exam> Exams { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<ExamAttendance> ExamAttendances { get;set; }  //student xam attended details 

        public DbSet<StudentAnswer> StudentAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Exam>()
                .HasOne(e => e.courseSubject)
                .WithMany(cs => cs.Exams)
                .HasForeignKey(e => e.CourseSubjectId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ExamAttendance>()
                .HasOne(ea => ea.Exam)
                .WithMany(e => e.ExamAttendances)
                .HasForeignKey(ea => ea.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.ExamAttendance)
                .WithMany(ea => ea.StudentAnswers)
                .HasForeignKey(sa => sa.ExamAttendanceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Question)
                .WithMany(q => q.StudentAnswers)
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
