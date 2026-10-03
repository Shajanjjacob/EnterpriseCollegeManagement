using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
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
    public class SubjectServiceTests
    {
        private readonly Mock<IStudentServiceClient> _studentServiceClientMock;
        private readonly Mock<ILogger<SubjectService>> _loggerMock;

        public SubjectServiceTests()
        {
            _loggerMock = new Mock<ILogger<SubjectService>>();
            _studentServiceClientMock = new Mock<IStudentServiceClient>();
        }

        public AcademicDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AcademicDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new AcademicDbContext(options);
        }

        [Fact]
        public async Task CreateSubjectAsync_ShouldCreateSubject_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Computer Science and Engineering"
            };


            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);


            var request = new CreateSubjectRequestDto
            {
                Code = "CS101",
                Name = "Programming Fundamentals",
                Credits = 4,
                DepartmentId = 1

            };

            var service = new SubjectService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            var result = await service.CreateSubjectAsync(request, "admin-001");

            Assert.NotNull(result);
            Assert.Equal("CS101", result.Code);
            Assert.Equal("Programming Fundamentals", result.Name);
            Assert.Equal(4, result.Credits);
            Assert.Equal(1, result.DepartmentId);
            Assert.Equal("Computer Science and Engineering", result.DepartmentName);


            var subjectexists = await context.Subjects.FirstOrDefaultAsync(x => x.Code == "CS101");


            Assert.NotNull(subjectexists);
            Assert.Equal("CS101", subjectexists.Code);
            Assert.Equal("Programming Fundamentals", subjectexists.Name);
            Assert.Equal(4, subjectexists.Credits);
            Assert.Equal(1, subjectexists.DepartmentId);

            Assert.Equal("admin-001", subjectexists.CreatedBy);
            Assert.NotNull(subjectexists.CreatedDate);
            Assert.False(subjectexists.IsDeleted);
        }
        [Fact]
        public async Task CreateSubjectAsync_ShouldThrowNotFoundException_WhenDepartmentDoesNotExist()
        {
            await using var context = CreateDbContext();

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync((DepartmentResponse?)null);

            var request = new CreateSubjectRequestDto
            {
                Code = "CS101",
                Name = "Programming Fundamentals",
                Credits = 4,
                DepartmentId = 1
            };

            var service = new SubjectService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateSubjectAsync(request, "admin-001"));

            var savedSubject = await context.Subjects.FirstOrDefaultAsync(x => x.Code == "CS101");

            Assert.Null(savedSubject);
        }

        [Fact]
        public async Task CreateSubjectAsync_ShouldThrowConflictException_WhenSubjectCodeAlreadyExists()
        {
            await using var context = CreateDbContext();

            var existingsubject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Existing Subject",
                Credits = 4,
                DepartmentId = 1,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Add(existingsubject);
            await context.SaveChangesAsync();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Computer Science"
            };

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var subject = new CreateSubjectRequestDto
            {
                Code = "CS101",
                Name = "New Subject",
                Credits = 3,
                DepartmentId = 1
            };

            var service = new SubjectService(context, _loggerMock.Object, _studentServiceClientMock.Object);

           await Assert.ThrowsAsync<ConflictException>(() => service.CreateSubjectAsync(subject, "admin-002"));

            var subjects = await context.Subjects.ToListAsync();

            Assert.Single(subjects);
            Assert.Equal("Existing Subject", subjects[0].Name);
        }

        [Fact]
        public async Task GetSubjectByIdAsync_ShouldReturnSubject_WhenSubjectExists()
        {
            await using var context = CreateDbContext();

            var subject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Programming Fundamentals",
                Credits = 4,
                DepartmentId = 1,
                IsDeleted = false
            };

            context.Add(subject);
            await context.SaveChangesAsync();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Computer Science"
            };

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var service = new SubjectService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            var result = await service.GetSubjectByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("CS101", result.Code);
            Assert.Equal("Programming Fundamentals", result.Name);
            Assert.Equal(4, result.Credits);
            Assert.Equal(1, result.DepartmentId);
            Assert.Equal("Computer Science", result.DepartmentName);
        }

        [Fact]
        public async Task UpdateSubjectAsync_ShouldUpdateSubject_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var existingsubject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Programming Fundamentals",
                Credits = 3,
                DepartmentId = 1,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Add(existingsubject);
            await context.SaveChangesAsync();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Information Technology"
            };

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var request = new UpdateSubjectRequestDto
            {
                Code = "IT101",
                Name = "Advanced Programming",
                Credits = 4,
                DepartmentId = 1
            };

            var service = new SubjectService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            var result = await service.UpdateSubjectAsync(1, request, "admin-002");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("IT101", result.Code);
            Assert.Equal("Advanced Programming", result.Name);
            Assert.Equal(4, result.Credits);
            Assert.Equal("Information Technology", result.DepartmentName);

            var updatedSubject = await context.Subjects.FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(updatedSubject);
            Assert.Equal("IT101", updatedSubject.Code);
            Assert.Equal("Advanced Programming", updatedSubject.Name);
            Assert.Equal(4, updatedSubject.Credits);
            Assert.Equal("admin-002", updatedSubject.UpdatedBy);
            Assert.NotNull(updatedSubject.UpdatedDate);
            Assert.False(updatedSubject.IsDeleted);
        }

        [Fact]
        public async Task DeleteSubjectAsync_ShouldSoftDeleteSubject_WhenSubjectExists()
        {
            await using var context = CreateDbContext();

            var existingsubject = new Subject
            {
                Id = 1,
                Code = "CS101",
                Name = "Programming Fundamentals",
                Credits = 3,
                DepartmentId = 1,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Add(existingsubject);
            await context.SaveChangesAsync();

            var service = new SubjectService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            var result = await service.DeleteSubjectAsync("admin-002", 1);

            Assert.True(result);

            var deletedSubject = await context.Subjects.FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(deletedSubject);
            Assert.True(deletedSubject.IsDeleted);
            Assert.Equal("admin-002", deletedSubject.DeletedBy);
            Assert.NotNull(deletedSubject.DeletedDate);
        }
    }
}

