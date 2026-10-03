using EnterpriseCollegeManagement.AcademicService.Data;
using EnterpriseCollegeManagement.AcademicService.DTOs.Requests;
using EnterpriseCollegeManagement.AcademicService.DTOs.Responses;
using EnterpriseCollegeManagement.AcademicService.Entities;
using EnterpriseCollegeManagement.AcademicService.Exceptions;
using EnterpriseCollegeManagement.AcademicService.Interfaces;
using EnterpriseCollegeManagement.AcademicService.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Validations;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseCollegeManagement.AcademicService.Tests.Services
{
    public class CourseServiceTests
    {
        private readonly Mock<IStudentServiceClient> _studentServiceClientMock;
        private readonly Mock<ILogger<CourseService>> _loggerMock;

        public CourseServiceTests()
        {
            _studentServiceClientMock = new Mock<IStudentServiceClient>();
            _loggerMock = new Mock<ILogger<CourseService>>();
        }

        private AcademicDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AcademicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options; //for each test we are using new database

            return new AcademicDbContext(options);
        }

        [Fact]

        public async Task CreateCourseAsync_ShouldCreateCourse_WhenRequestIsValid()
        {
           await using var context = CreateDbContext();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Computer Science and Engineering"
            };

            _studentServiceClientMock.Setup( x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var request = new CreateCourseRequest
            {
                Code = "CSE",
                Name = "Bachelor of Computer Applications",
                Description = "Computer Application Course",
                DurationYears = 3,
                DepartmentId = 1
            };

            var service = new CourseService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            var result = await service.CreateCourseAsync(request, "admin-001");

            Assert.NotNull(result);
            Assert.Equal("CSE", result.Code);
            Assert.Equal("Bachelor of Computer Applications", result.Name);
            Assert.Equal(1, result.DepartmentId);
            Assert.Equal("Computer Science and Engineering", result.DepartmentName);

            var newCourse = await context.Courses.FirstOrDefaultAsync(x => x.Code == "CSE");

            Assert.NotNull(newCourse);
            Assert.Equal("CSE", newCourse.Code);
            Assert.Equal("Bachelor of Computer Applications", newCourse.Name);
            Assert.Equal("Computer Application Course", newCourse.Description);
            Assert.Equal(3, newCourse.DurationYears);
            Assert.Equal(1, newCourse.DepartmentId);

            Assert.Equal("admin-001", newCourse.CreatedBy);
            Assert.NotNull(newCourse.CreatedDate);
            Assert.False(newCourse.IsDeleted);
        }

        [Fact]
        public async Task CreateCourseAsync_ShouldThrowNotFoundException_WhenDepartmentDoesNotExist()
        {
            await using var context = CreateDbContext();

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync((DepartmentResponse?)null);

            var request = new CreateCourseRequest
            {
                Code = "CSE",
                Name = "Bachelor of Computer Applications",
                Description = "Computer Application Course",
                DurationYears = 3,
                DepartmentId = 1
            };


            var service = new CourseService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateCourseAsync(request, "admin-001"));

            var course = await context.Courses.FirstOrDefaultAsync(x => x.Code == "CSE");
            Assert.Null(course);
        }

        [Fact]
        public async Task CreateCourseAsync_ShouldThrowConflictException_WhenCourseCodeAlreadyExists()
        {
            await using var context = CreateDbContext();

            var existingcourse = new Course
            {
                Id = 1,
                Code = "CSE",
                Name = "Existing Course",
                Description = "Existing Description",
                DurationYears = 3,
                DepartmentId = 1,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Courses.Add(existingcourse);
            await context.SaveChangesAsync();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Computer Science and Engineering"

            };

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var request = new CreateCourseRequest
            {
                Code = "CSE",
                Name = "New Course",
                Description = "New Description",
                DurationYears = 3,
                DepartmentId = 1
            };


            var service = new CourseService(context, _loggerMock.Object, _studentServiceClientMock.Object);

            await Assert.ThrowsAsync<ConflictException>(() => service.CreateCourseAsync(request, "admin-002"));

            var courses = await context.Courses.ToListAsync();

            Assert.Single(courses);
            Assert.Equal("Existing Course", courses[0].Name);
        }

        [Fact]
        public async Task GetCourseByIdAsync_ShouldReturnCourse_WhenCourseExists()
        {
            await using var context = CreateDbContext();

            var request = new Course
            {
                Id = 1,
                Code = "CSE",
                Name = "Computer Science",
                Description = "Computer Science Course",
                DurationYears = 4,
                DepartmentId = 1,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Courses.Add(request);
            await context.SaveChangesAsync();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Computer Science and Engineering"
            };

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var service = new CourseService(context,_loggerMock.Object, _studentServiceClientMock.Object);

            var result = await service.GetCourseByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("CSE", result.Code);
            Assert.Equal("Computer Science", result.Name);
            Assert.Equal("Computer Science Course", result.Description);
            Assert.Equal(4, result.DurationYears);
            Assert.Equal(1, result.DepartmentId);
            Assert.Equal("Computer Science and Engineering", result.DepartmentName);
        }

        [Fact]
        public async Task UpdateCourseAsync_ShouldUpdateCourse_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var existingcourse = new Course
            {
                Id = 1,
                Code = "CSE",
                Name = "Computer Science",
                Description = "Old Description",
                DurationYears = 3,
                DepartmentId = 1,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false

            };

            context.Add(existingcourse);
            await context.SaveChangesAsync();

            var department = new DepartmentResponse
            {
                Id = 1,
                Name = "Information Technology Department"
            };

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(1)).ReturnsAsync(department);

            var request = new UpdateCourseRequestDto
            {
               
                Code = "IT",
                Name = "Information Technology",
                Description = "Updated Description",
                DurationYears = 4,
                DepartmentId = 1
            };

            var service = new CourseService(context,_loggerMock.Object,_studentServiceClientMock.Object);

            var result = await service.UpdateCourseAsync(request, "admin-002",1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("IT", result.Code);
            Assert.Equal("Information Technology", result.Name);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(4, result.DurationYears);
            Assert.Equal(1, result.DepartmentId);
            Assert.Equal("Information Technology Department", result.DepartmentName);

            var updatedCourse = await context.Courses.FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(updatedCourse);
            Assert.Equal("IT", updatedCourse.Code);
            Assert.Equal("Information Technology", updatedCourse.Name);
            Assert.Equal("Updated Description", updatedCourse.Description);
            Assert.Equal(4, updatedCourse.DurationYears);

            Assert.Equal("admin-002", updatedCourse.UpdatedBy);
            Assert.NotNull(updatedCourse.UpdatedDate);
            Assert.False(updatedCourse.IsDeleted);
        }

        [Fact]
        public async Task DeleteCourseAsync_ShouldSoftDeleteCourse_WhenCourseExists()
        {
            await using var context = CreateDbContext();

            var course = new Course
            {
                Id = 1,
                Code = "CSE",
                Name = "Computer Science",
                Description = "Computer Science Course",
                DurationYears = 4,
                DepartmentId = 1,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var service = new CourseService(context,_loggerMock.Object, _studentServiceClientMock.Object);

            var result = await service.DeleteCourseAsync(1, "admin-002");

            Assert.True(result);

            var deletedCourse = await context.Courses.FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(deletedCourse);
            Assert.True(deletedCourse.IsDeleted);
            Assert.Equal("admin-002", deletedCourse.DeletedBy);
            Assert.NotNull(deletedCourse.DeletedDate);
        }
    }
}
