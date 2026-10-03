using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseCollegeManagement.AcademicService.Tests.Services
{
    public class ExamServiceTests
    {
        private readonly Mock<ILogger<ExamService>> _loggerMock;


        public ExamServiceTests()
        {
            _loggerMock = new Mock<ILogger<ExamService>>();
        }


        private AcademicDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AcademicDbContext>()
              .UseInMemoryDatabase(Guid.NewGuid().ToString())
              .Options;

            return new AcademicDbContext(options);
        }
        [Fact]
        public async Task CreateExamAsync_ShouldCreateExam_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

           var course = new Course
                {
                    Id = 1,
                    Code = "BCA",
                    Name = "Bachelor of Computer Applications",
                    IsDeleted = false
                };

            var subject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Programming Fundamentals",
                IsDeleted = false
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course,
                Subject = subject
            };

            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.CoursesSubjects.Add(courseSubject);

            await context.SaveChangesAsync();

            var request = new CreateExamRequestDto
            {
                CourseSubjectId = 1,
                Title = "Programming Fundamentals Midterm",
                Description = "Midterm examination",
                DurationMinutes = 60,
                TotalMarks = 50
            };

            var service = new ExamService(_loggerMock.Object,context);

            var result = await service.CreateExamAsync(request, "teacher-001");

            Assert.NotNull(result);
            Assert.Equal(1, result.CourseSubjectId);
            Assert.Equal("Programming Fundamentals Midterm", result.Title);
            Assert.Equal("Midterm examination", result.Description);
            Assert.Equal(60, result.DurationMinutes);
            Assert.Equal(50, result.TotalMarks);
            Assert.Equal("Bachelor of Computer Applications", result.CourseName);
            Assert.Equal("Programming Fundamentals", result.SubjectName);
            Assert.Equal(1, result.Semester);
            Assert.False(result.IsPublished);

            var savedExam = await context.Exams.FirstOrDefaultAsync(x => x.Id == result.Id);

            Assert.NotNull(savedExam);
            Assert.Equal("Programming Fundamentals Midterm", savedExam.Title);
            Assert.Equal("teacher-001", savedExam.CreatedBy);
            Assert.NotNull(savedExam.CreatedDate);
            Assert.False(savedExam.IsDeleted);
            Assert.False(savedExam.IsPublished);
        }

        [Fact]
        public async Task CreateExamAsync_ShouldThrowNotFoundException_WhenCourseSubjectDoesNotExist()
        {
            await using var context = CreateDbContext();

            var request = new CreateExamRequestDto
            {
                CourseSubjectId = 999,
                Title = "Midterm Exam",
                Description = "Test exam",
                DurationMinutes = 60,
                TotalMarks = 50
            };

            var service = new ExamService(_loggerMock.Object, context);

            await Assert.ThrowsAsync<NotFoundException>( () => service.CreateExamAsync(request, "teacher-001"));

            var exams = await context.Exams.ToListAsync();

            Assert.Empty(exams);
        }

        [Fact]
        public async Task GetExamByIdAsync_ShouldReturnExam_WhenExamExists()
        {
            await using var context = CreateDbContext();

            var course = new Course
            {
                Id = 1,
                Code = "BCA",
                Name = "Bachelor of Computer Applications",
                IsDeleted = false
            };

            var subject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Programming Fundamentals",
                IsDeleted = false
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course,
                Subject = subject
            };

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "Midterm Exam",
                Description = "Midterm",
                DurationMinutes = 60,
                TotalMarks = 50,
                IsPublished = false,
                IsDeleted = false
            };

            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            var service = new ExamService(_loggerMock.Object,context);

            var result = await service.GetExamByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Midterm Exam", result.Title);
            Assert.Equal("Bachelor of Computer Applications", result.CourseName);
            Assert.Equal("Programming Fundamentals", result.SubjectName);
            Assert.Equal(1, result.Semester);
            Assert.Equal(60, result.DurationMinutes);
            Assert.Equal(50, result.TotalMarks);
        }

        [Fact]
        public async Task UpdateExamAsync_ShouldUpdateExam_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var course = new Course
            {
                Id = 1,
                Code = "BCA",
                Name = "Bachelor of Computer Applications",
                IsDeleted = false
            };

            var subject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Programming Fundamentals",
                IsDeleted = false

            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course,
                Subject = subject

            };

            var existingExam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "Old Exam",
                Description = "Old Description",
                DurationMinutes = 60,
                TotalMarks = 50,
                IsPublished = false,
                IsDeleted = false,
                CreatedBy = "teacher-001",
                CreatedDate = DateTime.UtcNow
            };

            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(existingExam);
            await context.SaveChangesAsync();

            var service = new ExamService(_loggerMock.Object, context);


            var request = new CreateExamRequestDto
            {
                CourseSubjectId = 1,
                Title = "Updated Exam",
                Description = "Updated Description",
                DurationMinutes = 90,
                TotalMarks = 100
            };

            var result = await service.UpdateExamAsync(1, request, "teacher-002");

            var updatedExam = await context.Exams .FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(result);
            Assert.Equal("Updated Exam", result.Title);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(90, result.DurationMinutes);
            Assert.Equal(100, result.TotalMarks);

            Assert.NotNull(updatedExam);
            Assert.Equal("Updated Exam", updatedExam.Title);
            Assert.Equal(100, updatedExam.TotalMarks);
            Assert.Equal("teacher-002", updatedExam.UpdatedBy);
            Assert.NotNull(updatedExam.UpdatedDate);

        }


        [Fact]
        public async Task DeleteExamAsync_ShouldSoftDeleteExam_WhenExamExists()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "Midterm Exam",
                DurationMinutes = 60,
                TotalMarks = 50,
                IsPublished = false,
                IsDeleted = false,
                CreatedBy = "teacher-001",
                CreatedDate = DateTime.UtcNow,
                Description = "Old Description"

            };

            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            var service = new ExamService(_loggerMock.Object,context);

            var result = await service.DeleteExamAsync(1,"teacher-002");

            Assert.True(result);

            var deletedExam = await context.Exams.FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(deletedExam);
            Assert.True(deletedExam.IsDeleted);
            Assert.Equal("teacher-002", deletedExam.DeletedBy);
            Assert.NotNull(deletedExam.DeletedDate);
        }

        [Fact]
        public async Task PublishExamAsync_ShouldPublishExam_WhenQuestionMarksMatchExamTotalMarks()
        {
            await using var context = CreateDbContext();

            var course = new Course
            {
                Id = 1,
                Code = "BCA",
                Name = "Bachelor of Computer Applications",
                IsDeleted = false
            };

            var subject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Programming Fundamentals",
                IsDeleted = false
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course,
                Subject = subject
            };

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "Programming Fundamentals Exam",
                Description = "Internal Examination",
                DurationMinutes = 30,
                TotalMarks = 10,
                IsPublished = false,
                IsDeleted = false,
                CreatedBy = "teacher-001",
                CreatedDate = DateTime.UtcNow,
                courseSubject = courseSubject  //relation
            };

            var question = new Question   // qustn marks == exam total mark 
            {
                Id = 1,
                ExamId = 1,
                Exam = exam,  //relation
                QuestionText = "What is a class in C#?",
                OptionA = "A blueprint for objects",
                OptionB = "A database",
                OptionC = "A namespace",
                OptionD = "A loop",
                CorrectOption = "A",
                Marks = 10,
                IsDeleted = false,
                CreatedBy = "teacher-001",
                CreatedDate = DateTime.UtcNow,
              
            };

            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);
            context.Questions.Add(question);
            await context.SaveChangesAsync();

            var service = new ExamService(_loggerMock.Object, context);

            var result = await service.PublishExamAsync(1, "teacher-001");

            var publishedExam = await context.Exams.FirstOrDefaultAsync(x => x.Id == 1);


            Assert.NotNull(result);

            Assert.Equal(1, result.Id);
            Assert.Equal("Programming Fundamentals Exam", result.Title);
            Assert.Equal(10, result.TotalMarks);

            Assert.True(result.IsPublished);

           

            Assert.NotNull(publishedExam);

            Assert.True(publishedExam.IsPublished);

            Assert.Equal("teacher-001", publishedExam.UpdatedBy);

            Assert.NotNull(publishedExam.UpdatedDate);

        }
    }
}
