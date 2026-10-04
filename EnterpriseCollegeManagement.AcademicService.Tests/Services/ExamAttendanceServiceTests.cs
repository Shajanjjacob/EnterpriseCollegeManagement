using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using EnterpriseCollegeManagement.AcademicService.Services;
using Google.GenAI.Types;
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
    public class ExamAttendanceServiceTests
    {
        private readonly Mock<ILogger<ExamAttendanceService>> _loggerMock;
        private readonly Mock<IStudentServiceClient> _studentServiceMock;

        public ExamAttendanceServiceTests()
        {
            _loggerMock = new Mock<ILogger<ExamAttendanceService>>();
            _studentServiceMock = new Mock<IStudentServiceClient>();
        }

        private AcademicDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AcademicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AcademicDbContext(options);
        }


        [Fact]
        public async Task StartExamAsync_ShouldCreateAttendance_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var student = new StudentResponseDto
            {
                UserId = "student-1",
                AdmissionNumber = "ADM001",
                CourseId = 1,
                DepartmentId = 1,
                Semester = 1
            };

            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course
            };

            context.Courses.Add(course);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1")).ReturnsAsync(student);

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 1
            };
            var result = await service.StartExamAsync(request, "student-1");

            var attendance = await context.ExamAttendances.FirstOrDefaultAsync(x => x.Id == result.Id);

            Assert.NotNull(result);
            Assert.Equal(1, result.ExamId);
            Assert.Equal("student-1", result.StudentUserId);
            Assert.Equal(10, result.TotalMarks);
            Assert.Equal(0, result.Score);
            Assert.False(result.IsSubmitted);
            Assert.NotNull(attendance);
            Assert.Equal(1, attendance.ExamId);
            Assert.Equal("student-1", attendance.StudentUserId);
            Assert.False(attendance.IsSubmitted);
        }

        [Fact]
        public async Task StartExamAsync_ShouldThrowNotFoundException_WhenStudentDoesNotExist()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };



            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course
            };

            context.Courses.Add(course);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 1
            };


            await Assert.ThrowsAsync<NotFoundException>(() => service.StartExamAsync(request, "student-1"));


        }

        [Fact]
        public async Task StartExamAsync_ShouldThrowBadRequestException_WhenExamIsNotPublished()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = false,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var student = new StudentResponseDto
            {
                UserId = "student-1",
                AdmissionNumber = "ADM001",
                CourseId = 1,
                DepartmentId = 1,
                Semester = 1
            };

            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course
            };

            context.Courses.Add(course);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1")).ReturnsAsync(student);

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 1
            };

            await Assert.ThrowsAsync<BadRequestException>(() => service.StartExamAsync(request, "student-1"));
        }

        [Fact]
        public async Task StartExamAsync_ShouldThrowConflictException_WhenStudentAlreadyStarted()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var student = new StudentResponseDto
            {
                UserId = "student-1",
                AdmissionNumber = "ADM001",
                CourseId = 1,
                DepartmentId = 1,
                Semester = 1
            };

            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course
            };

            var attendance = new ExamAttendance  //existing one 
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                ExpiresAt = DateTime.UtcNow.AddMinutes(50),
                Score = 0,
                TotalMarks = 10,
                IsSubmitted = false
            };

            context.ExamAttendances.Add(attendance);
            context.Courses.Add(course);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1")).ReturnsAsync(student);

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 1
            };

            await Assert.ThrowsAsync<ConflictException>(() => service.StartExamAsync(request, "student-1"));

        }

        [Fact]
        public async Task SubmitExamAsync_ShouldSubmitExam_WhenAllQuestionsAreAnswered()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                ExpiresAt = DateTime.UtcNow.AddMinutes(50),
                Score = 0,
                TotalMarks = 10,
                IsSubmitted = false
            };

            var question1 = new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "Question 1",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var question2 = new Question
            {
                Id = 2,
                ExamId = 1,
                QuestionText = "Question 2",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = "B",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var answer1 = new StudentAnswer
            {
                Id = 1,
                ExamAttendanceId = 1,
                QuestionId = 1,
                MarksAwarded = 5
            };

            var answer2 = new StudentAnswer
            {
                Id = 2,
                ExamAttendanceId = 1,
                QuestionId = 2,
                MarksAwarded = 3
            };

            context.Exams.Add(exam);
            context.ExamAttendances.Add(attendance);
            context.Questions.AddRange(question1, question2);
            context.StudentAnswers.AddRange(answer1, answer2);

            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);


            var result = await service.SubmitExamAsync("student-1", 1);

            var savedAttendance = await context.ExamAttendances.FirstAsync(x => x.Id == 1);


            Assert.NotNull(result);
            Assert.True(result.IsSubmitted);
            Assert.Equal(8, result.Score);
            Assert.Equal(10, result.TotalMarks);
            Assert.NotNull(result.SubmittedAt);
            Assert.True(savedAttendance.IsSubmitted);
            Assert.Equal(8, savedAttendance.Score);
        }

        [Fact]
        public async Task SubmitExamAsync_ShouldThrowBadRequestException_WhenNotAllQuestionsAreAnswered()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                ExpiresAt = DateTime.UtcNow.AddMinutes(50),
                Score = 0,
                TotalMarks = 10,
                IsSubmitted = false
            };

            var question1 = new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "Question 1",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var question2 = new Question
            {
                Id = 2,
                ExamId = 1,
                QuestionText = "Question 2",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = "B",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var answer1 = new StudentAnswer
            {
                Id = 1,
                ExamAttendanceId = 1,
                QuestionId = 1,
                MarksAwarded = 5
            };



            context.Exams.Add(exam);
            context.ExamAttendances.Add(attendance);
            context.Questions.AddRange(question1, question2);


            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() => service.SubmitExamAsync("student-1", 1));
        }
        [Fact]
        public async Task SubmitExamAsync_ShouldThrowConflictException_WhenExamAlreadySubmitted()
        {
            await using var context = CreateDbContext();

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                ExpiresAt = DateTime.UtcNow.AddMinutes(50),
                Score = 8,
                TotalMarks = 10,
                IsSubmitted = true,
                SubmittedAt = DateTime.UtcNow
            };

            context.ExamAttendances.Add(attendance);

            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);


            await Assert.ThrowsAsync<ConflictException>(() => service.SubmitExamAsync("student-1", 1));
        }

        [Fact]
        public async Task PublishResultAsync_ShouldPublishResult_WhenExamIsSubmitted()
        {
            await using var context = CreateDbContext();

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddMinutes(-60),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                Score = 8,
                TotalMarks = 10,
                IsSubmitted = true,
                SubmittedAt = DateTime.UtcNow
            };

            context.ExamAttendances.Add(attendance);

            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);


            var result = await service.PublishResultAsync(1, "admin-1");

            var savedAttendance = await context.ExamAttendances.FirstAsync(x => x.Id == 1);

            Assert.NotNull(result);
            Assert.True(result.IsResultPublished);
            Assert.Equal("admin-1", result.ResultPublishedBy);
            Assert.NotNull(result.ResultPublishedDate);
            Assert.True(savedAttendance.IsResultPublished);
            Assert.Equal("admin-1", savedAttendance.ResultPublishedBy);

        }

        [Fact]
        public async Task PublishResultAsync_ShouldThrowBadRequestException_WhenExamIsNotSubmitted()
        {
            await using var context = CreateDbContext();

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddMinutes(-60),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                Score = 8,
                TotalMarks = 10,
                IsSubmitted = false
                
            };

            context.ExamAttendances.Add(attendance);

            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() => service.PublishResultAsync(1, "admin"));
        }



        [Fact]
        public async Task StartExamAsync_ShouldThrowNotFoundException_WhenExamDoesNotExist()
        {
            await using var context = CreateDbContext();

            var student = new StudentResponseDto
            {
                UserId = "student-1",
                AdmissionNumber = "ADM001",
                CourseId = 1,
                DepartmentId = 1,
                Semester = 1
            };

            _studentServiceMock
                .Setup(x => x.GetStudentByUserIdAsync("student-1"))
                .ReturnsAsync(student);

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 999
            };

            await Assert.ThrowsAsync<NotFoundException>(() =>service.StartExamAsync(request, "student-1"));
        }


        [Fact]
        public async Task StartExamAsync_ShouldThrowBadRequestException_WhenStudentCourseDoesNotMatch()
        {
            await using var context = CreateDbContext();

            var student = new StudentResponseDto
            {
                UserId = "student-1",
                AdmissionNumber = "ADM001",
                CourseId = 2,
                DepartmentId = 1,
                Semester = 1
            };

            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course
            };

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                Description = "Description",
               
            };

            context.Courses.Add(course);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1")).ReturnsAsync(student);

            var service = new ExamAttendanceService(context,_loggerMock.Object,_studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 1
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>service.StartExamAsync(request, "student-1"));
        }


        [Fact]
        public async Task StartExamAsync_ShouldThrowBadRequestException_WhenStudentDepartmentDoesNotMatch()
        {
            await using var context = CreateDbContext();

            var student = new StudentResponseDto
            {
                UserId = "student-1",
                AdmissionNumber = "ADM001",
                CourseId = 1,
                DepartmentId = 2,
                Semester = 1
            };

            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course
            };

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "test",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            context.Courses.Add(course);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1")).ReturnsAsync(student);

            var service = new ExamAttendanceService(context, _loggerMock.Object,_studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 1
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>service.StartExamAsync(request, "student-1"));
        }


        [Fact]
        public async Task StartExamAsync_ShouldThrowBadRequestException_WhenStudentSemesterDoesNotMatch()
        {
            await using var context = CreateDbContext();

            var student = new StudentResponseDto
            {
                UserId = "student-1",
                AdmissionNumber = "ADM001",
                CourseId = 1,
                DepartmentId = 1,
                Semester = 2
            };

            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var courseSubject = new CourseSubject
            {
                Id = 1,
                CourseId = 1,
                SubjectId = 1,
                Semester = 1,
                Course = course
            };

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "xyz",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            context.Courses.Add(course);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);

            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1")).ReturnsAsync(student);

            var service = new ExamAttendanceService(context,_loggerMock.Object,_studentServiceMock.Object);

            var request = new CreateExamAttendanceRequestDto
            {
                ExamId = 1
            };

            await Assert.ThrowsAsync<BadRequestException>(() =>service.StartExamAsync(request, "student-1"));
        }


        [Fact]
        public async Task GetExamQuestionsAsync_ShouldReturnQuestions_WhenAttendanceIsValid()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "xyss",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                TotalMarks = 10,
                IsSubmitted = false
            };

            var question = new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "What is C#?",
                OptionA = "Language",
                OptionB = "Database",
                OptionC = "OS",
                OptionD = "Browser",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = true
            };

            context.Exams.Add(exam);
            context.ExamAttendances.Add(attendance);
            context.Questions.Add(question);

            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1"))
                .ReturnsAsync(new StudentResponseDto
                {
                    UserId = "student-1",
                    AdmissionNumber = "ADM001",
                    CourseId = 1,
                    DepartmentId = 1,
                    Semester = 1
                });

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            var result = await service.GetExamQuestionsAsync(1,"student-1");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("What is C#?", result[0].QuestionText);
        }


        [Fact]
        public async Task SubmitExamAsync_ShouldAutoSubmit_WhenExamHasExpired()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                TotalMarks = 10,
                Score = 5,
                IsSubmitted = false
            };

            var question = new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "Question",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var answer = new StudentAnswer
            {
                Id = 1,
                ExamAttendanceId = 1,
                QuestionId = 1,
                MarksAwarded = 5
            };

            context.Exams.Add(exam);
            context.ExamAttendances.Add(attendance);
            context.Questions.Add(question);
            context.StudentAnswers.Add(answer);

            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object,_studentServiceMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() =>service.SubmitExamAsync("student-1", 1));

            var savedAttendance = await context.ExamAttendances .FirstAsync(x => x.Id == 1);

            Assert.True(savedAttendance.IsSubmitted);
            Assert.Equal(5, savedAttendance.Score);
        }


        [Fact]
        public async Task PublishResultAsync_ShouldThrowConflictException_WhenResultAlreadyPublished()
        {
            await using var context = CreateDbContext();

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(-1),
                SubmittedAt = DateTime.UtcNow.AddHours(-1),
                Score = 8,
                TotalMarks = 10,
                IsSubmitted = true,
                IsResultPublished = true,
                ResultPublishedBy = "admin-1",
                ResultPublishedDate = DateTime.UtcNow
            };

            context.ExamAttendances.Add(attendance);
            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            await Assert.ThrowsAsync<ConflictException>(() => service.PublishResultAsync(1, "admin-2"));
        }


        [Fact]
        public async Task GetStudentResultAsync_ShouldReturnNull_WhenAttendanceDoesNotExist()
        {
            await using var context = CreateDbContext();

            var service = new ExamAttendanceService(context,_loggerMock.Object,_studentServiceMock.Object);

            var result = await service.GetStudentResultAsync(1, "student-1");

            Assert.Null(result);
        }


        [Fact]
        public async Task GetStudentResultAsync_ShouldThrowBadRequestException_WhenExamIsNotSubmitted()
        {
            await using var context = CreateDbContext();

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                TotalMarks = 10,
                IsSubmitted = false,
                IsResultPublished = false
            };

            context.ExamAttendances.Add(attendance);
            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context,_loggerMock.Object,_studentServiceMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() =>service.GetStudentResultAsync(1,"student-1"));
        }


        [Fact]
        public async Task GetStudentResultAsync_ShouldThrowBadRequestException_WhenResultIsNotPublished()
        {
            await using var context = CreateDbContext();

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(-1),
                SubmittedAt = DateTime.UtcNow.AddHours(-1),
                Score = 8,
                TotalMarks = 10,
                IsSubmitted = true,
                IsResultPublished = false
            };

            context.ExamAttendances.Add(attendance);
            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context, _loggerMock.Object, _studentServiceMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() => service.GetStudentResultAsync(1, "student-1"));
        }


        [Fact]
        public async Task GetExamSubmissionsAsync_ShouldReturnSubmittedStudents()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(-1),
                SubmittedAt = DateTime.UtcNow.AddHours(-1),
                Score = 8,
                TotalMarks = 10,
                IsSubmitted = true,
                IsResultPublished = true
            };

            context.Exams.Add(exam);
            context.ExamAttendances.Add(attendance);
            await context.SaveChangesAsync();

            _studentServiceMock.Setup(x => x.GetStudentByUserIdAsync("student-1"))
                .ReturnsAsync(new StudentResponseDto
                {
                    UserId = "student-1",
                    AdmissionNumber = "ADM001",
                    CourseId = 1,
                    DepartmentId = 1,
                    Semester = 1
                });

            var service = new ExamAttendanceService(context,_loggerMock.Object,_studentServiceMock.Object);

            var result = await service.GetExamSubmissionsAsync(1);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("ADM001", result[0].AdmissionNumber);
            Assert.Equal(8, result[0].Score);
            Assert.True(result[0].ExamSubmitted);
        }


        [Fact]
        public async Task GetStudentResultsAsync_ShouldReturnPublishedResults()
        {
            await using var context = CreateDbContext();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Computer Science"
            };

            var course = new Course
            {
                Id = 1,
                Name = "B.Tech CSE",
                DepartmentId = 1
            };

            var subject = new Subject
            {
                Id = 1,
                Name = "C# Programming"
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
                Title = "C# Fundamentals",
                Description = "sdxyz",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                courseSubject = courseSubject
            };

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(-1),
                SubmittedAt = DateTime.UtcNow.AddHours(-1),
                Score = 8,
                TotalMarks = 10,
                IsSubmitted = true,
                IsResultPublished = true
            };

           
            context.Courses.Add(course);
            context.Subjects.Add(subject);
            context.CoursesSubjects.Add(courseSubject);
            context.Exams.Add(exam);
            context.ExamAttendances.Add(attendance);

            await context.SaveChangesAsync();

            _studentServiceMock .Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var service = new ExamAttendanceService(context,_loggerMock.Object,_studentServiceMock.Object);

            var result = await service.GetStudentResultsAsync("student-1");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].ExamId);
            Assert.Equal("C# Fundamentals", result[0].ExamTitle);
            Assert.Equal("B.Tech CSE", result[0].CourseName);
            Assert.Equal("C# Programming", result[0].SubjectName);
            Assert.Equal("Computer Science", result[0].DepartmentName);
            Assert.Equal(8, result[0].Score);
        }


        [Fact]
        public async Task ProcessExpiredExamsAsync_ShouldAutoSubmitExpiredExam()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Description",
                DurationMinutes = 60,
                TotalMarks = 10,
                IsPublished = true,
                IsDeleted = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow
            };

            var attendance = new ExamAttendance
            {
                Id = 1,
                ExamId = 1,
                StudentUserId = "student-1",
                StartedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
                Score = 5,
                TotalMarks = 10,
                IsSubmitted = false
            };

            context.Exams.Add(exam);
            context.ExamAttendances.Add(attendance);

            await context.SaveChangesAsync();

            var service = new ExamAttendanceService(context,_loggerMock.Object,_studentServiceMock.Object);

            await service.ProcessExpiredExamsAsync();

            var savedAttendance = await context.ExamAttendances
                .FirstAsync(x => x.Id == 1);

            Assert.True(savedAttendance.IsSubmitted);
            Assert.Equal(10, savedAttendance.TotalMarks);
            Assert.Equal(savedAttendance.ExpiresAt, savedAttendance.SubmittedAt);
        }

    }
}
