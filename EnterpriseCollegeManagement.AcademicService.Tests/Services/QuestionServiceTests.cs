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
    public class QuestionServiceTests
    {
        private readonly Mock<ILogger<QuestionService>> _loggerMock;

        public QuestionServiceTests()
        {
            _loggerMock = new Mock<ILogger<QuestionService>>();
        }

        private AcademicDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AcademicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AcademicDbContext(options);
        }

        [Fact]
        public async Task CreateQuestionAsync_ShouldCreateQuestion_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
               Description = "Testing",
                TotalMarks = 10,
                DurationMinutes = 60,
                IsPublished = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            var request = new CreateQuestionRequestDto
            {
                ExamId = 1,
                QuestionText = "What is encapsulation?",
                OptionA = "Inheritance",
                OptionB = "Wrapping data and methods",
                OptionC = "Polymorphism",
                OptionD = "Abstraction",
                CorrectOption = "B",
                Marks = 5
            };


            var service = new QuestionService(context, _loggerMock.Object);

            var result = await service.CreateQuestionAsync(request, "teacher-1");

            var question = await context.Questions.FirstOrDefaultAsync(x => x.Id == result.Id);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal(1, result.ExamId);
            Assert.Equal("What is encapsulation?", result.QuestionText);
            Assert.Equal(5, result.Marks);
            Assert.NotNull(question);
            Assert.Equal("teacher-1", question.CreatedBy);
            Assert.False(question.IsDeleted);
        }

        [Fact]
        public async Task CreateQuestionAsync_ShouldThrowBadRequestException_WhenMarksAreInvalid()
        {
            await using var context = CreateDbContext();

            

            var request = new CreateQuestionRequestDto
            {
                ExamId = 1,
                QuestionText = "What is encapsulation?",
                OptionA = "Inheritance",
                OptionB = "Wrapping data and methods",
                OptionC = "Polymorphism",
                OptionD = "Abstraction",
                CorrectOption = "B",
                Marks = 0 //
            };

            var service = new QuestionService(context, _loggerMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() => service.CreateQuestionAsync(request, "teacher-1"));

        }
        [Fact]
        public async Task CreateQuestionAsync_ShouldThrowNotFoundException_WhenExamDoesNotExist()
        {
            await using var context = CreateDbContext();

            var request = new CreateQuestionRequestDto
            {
                ExamId = 22, //invalid
                QuestionText = "What is encapsulation?",
                OptionA = "Inheritance",
                OptionB = "Wrapping data and methods",
                OptionC = "Polymorphism",
                OptionD = "Abstraction",
                CorrectOption = "B",
                Marks = 5
            };

            var service = new QuestionService(context, _loggerMock.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateQuestionAsync(request, "teacher-1"));
        }

        [Fact]
        public async Task CreateQuestionAsync_ShouldThrowBadRequestException_WhenExamIsPublished()
        {
           
            using var context = CreateDbContext();
            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Testing",
                TotalMarks = 10,
                DurationMinutes = 60,
                IsPublished = true, // status changed 
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Exams.Add(exam);
            await context.SaveChangesAsync();

            var request = new CreateQuestionRequestDto
            {
                ExamId = 1, //invalid
                QuestionText = "What is encapsulation?",
                OptionA = "Inheritance",
                OptionB = "Wrapping data and methods",
                OptionC = "Polymorphism",
                OptionD = "Abstraction",
                CorrectOption = "B",
                Marks = 5
            };

            var service = new QuestionService(context,_loggerMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() => service.CreateQuestionAsync(request, "teacher-1"));
        }

        [Fact]
        public async Task GetQuestionByIdAsync_ShouldReturnQuestion_WhenQuestionExists()
        {
           
            using var context = CreateDbContext();

            var question = new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "What is C#?",
                OptionA = "Language",
                OptionB = "Database",
                OptionC = "OS",
                OptionD = "Framework",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Questions.Add(question);
            await context.SaveChangesAsync();

            var service = new QuestionService(context, _loggerMock.Object);

           
            var result = await service.GetQuestionByIdAsync(1);
          
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("What is C#?", result.QuestionText);
            Assert.Equal(5, result.Marks);
        }

        [Fact]
        public async Task GetQuestionsByExamIdAsync_ShouldReturnQuestions_WhenExamExists()
        {
           
            using var context = CreateDbContext();

            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Testing",
                TotalMarks = 10,
                DurationMinutes = 60,
                IsPublished = true, 
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Exams.Add(exam);

            context.Questions.AddRange(
                new Question
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
                },
                new Question
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
                });

            await context.SaveChangesAsync();

            var service = new QuestionService(context, _loggerMock.Object);

            
            var result = await service.GetQuestionsByExamIdAsync(1);

            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }

        [Fact]
        public async Task UpdateQuestionAsync_ShouldUpdateQuestion_WhenRequestIsValid()
        {
            using var context = CreateDbContext();
            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Testing",
                TotalMarks = 10,
                DurationMinutes = 60,
                IsPublished = false,
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Exams.Add(exam);


            var existingQuestion = new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "Old question",
                OptionA = "Old A",
                OptionB = "Old B",
                OptionC = "Old C",
                OptionD = "Old D",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Questions.Add(existingQuestion);
            await context.SaveChangesAsync();

            var request = new CreateQuestionRequestDto
            {
                
                ExamId = 1,
                QuestionText = "What is C#?",
                OptionA = "Language",
                OptionB = "Database",
                OptionC = "OS",
                OptionD = "Framework",
                CorrectOption = "A",
                Marks = 5,
               
            };
            var service = new QuestionService(context, _loggerMock.Object);

            var result = await service.UpdateQuestionAsync(1,request, "teacher-2");

            var question = await context.Questions.FirstAsync(x => x.Id == 1);

            Assert.NotNull(result);
            Assert.Equal("What is C#?", result.QuestionText);
            Assert.Equal(5, result.Marks);
            Assert.Equal("What is C#?", question.QuestionText);
            Assert.Equal("teacher-2", question.UpdatedBy);
            Assert.NotNull(question.UpdatedDate);
        }

        [Fact]
        public async Task UpdateQuestionAsync_ShouldThrowBadRequestException_WhenExamIsPublished()
        {
            using var context = CreateDbContext();
            var exam = new Exam
            {
                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Testing",
                TotalMarks = 10,
                DurationMinutes = 60,
                IsPublished = true, //changed here 
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Exams.Add(exam);


            var existingQuestion = new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "Old question",
                OptionA = "Old A",
                OptionB = "Old B",
                OptionC = "Old C",
                OptionD = "Old D",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Questions.Add(existingQuestion);
            await context.SaveChangesAsync();

            var request = new CreateQuestionRequestDto
            {

                ExamId = 1,
                QuestionText = "What is C#?",
                OptionA = "Language",
                OptionB = "Database",
                OptionC = "OS",
                OptionD = "Framework",
                CorrectOption = "A",
                Marks = 5,

            };
            var service = new QuestionService(context, _loggerMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() =>service.UpdateQuestionAsync(1, request,"teacher-2"));
        }

        [Fact]
        public async Task DeleteQuestionAsync_ShouldSoftDeleteQuestion_WhenQuestionExists()
        {
           
            using var context = CreateDbContext();

            context.Exams.Add(new Exam {

                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Testing",
                TotalMarks = 10,
                DurationMinutes = 60,
                IsPublished = false, 
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            });

            context.Questions.Add(new Question
            {
                Id = 1,
                ExamId = 1,
                QuestionText = "Question to delete",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = "A",
                Marks = 5,
                CreatedBy = "teacher-1",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            });

            await context.SaveChangesAsync();

            var service = new QuestionService(context,_loggerMock.Object);

         
            var result = await service.DeleteQuestionAsync(1,"teacher-2");

          
            Assert.True(result);

            var question = await context.Questions.FirstAsync(x => x.Id == 1);

            Assert.True(question.IsDeleted);
            Assert.Equal("teacher-2", question.DeletedBy);
            Assert.NotNull(question.DeletedDate);
        }

        [Fact]
        public async Task DeleteQuestionAsync_ShouldThrowBadRequestException_WhenExamIsPublished()
        {
         
            using var context = CreateDbContext();

            context.Exams.Add(new Exam
            {

                Id = 1,
                CourseSubjectId = 1,
                Title = "C# Fundamentals",
                Description = "Testing",
                TotalMarks = 10,
                DurationMinutes = 60,
                IsPublished = true, //changed 
                CreatedBy = "admin",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            });

            context.Questions.Add(new Question
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
            });

            await context.SaveChangesAsync();

            var service = new QuestionService(context, _loggerMock.Object);

            
            await Assert.ThrowsAsync<BadRequestException>(() =>service.DeleteQuestionAsync( 1, "teacher-2"));
        }
    }
}
