using EnterpriseCollegeManagement.TeacherService.Data;
using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;
using EnterpriseCollegeManagement.TeacherService.Entities;
using EnterpriseCollegeManagement.TeacherService.Exceptions;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using EnterpriseCollegeManagement.TeacherService.Services;
using EnterpriseCollegeManagement.TeacherService.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherServiceClass = EnterpriseCollegeManagement.TeacherService.Services.TeacherService;
namespace EnterpriseCollegeManagement.TeacherService.Test

{
    public class TeacherServiceTests
    {



        private readonly Mock<IStudentServiceClient> _studentServiceClientMock;
        private readonly Mock<IAcademicServiceClient> _academicServiceClientMock;
        private readonly Mock<IIdentityServiceClient> _identityServiceClientMock;
        private readonly Mock<IAuditServiceClient> _auditServiceClientMock;
        private readonly Mock<ILogger<TeacherServiceClass>> _loggerMock;

        public TeacherServiceTests()
        {
            _academicServiceClientMock = new Mock<IAcademicServiceClient>();
            _identityServiceClientMock = new Mock<IIdentityServiceClient>();
            _auditServiceClientMock = new Mock<IAuditServiceClient>();
            _studentServiceClientMock = new Mock<IStudentServiceClient>();
            _loggerMock =new Mock<ILogger<TeacherServiceClass>>();

        }

        private TeacherDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TeacherDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new TeacherDbContext(options);
        }

        [Fact]
        public async Task CreateTeacherAsync_ShouldCreateTeacher_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var identity = new TeacherUserResponseDto
            {
                Id = "user-123",
                Email = "teacher@test.com",
                Role = "Teacher"
            };

            _identityServiceClientMock.Setup(x=> x.GetUserByIdAsync("user-123")).ReturnsAsync(identity);

            var request = new CreateTeacherProfileRequestDto
            {
                UserId = "user-123",
                EmployeeCode = "T001",
                FirstName = "John",
                LastName = "Mathew",
                Phone = "9876543210"
            };


            var service = new TeacherServiceClass(_identityServiceClientMock.Object,_academicServiceClientMock.Object,_studentServiceClientMock.Object,
                            _auditServiceClientMock.Object,
                            context,
                            _loggerMock.Object);



            var result = await service.CreateTeacherAsync(request, "Admin1");

            Assert.NotNull(result);
            Assert.Equal("user-123", result.UserId);
            Assert.Equal("T001", result.EmployeeCode);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Mathew", result.LastName);

            var teacher = await context.Teachers.FirstOrDefaultAsync(x => x.UserId == "user-123");

            Assert.NotNull(teacher);
            Assert.Equal("T001", teacher.EmployeeCode);
            Assert.True(teacher.IsActive);
        }


        [Fact]
        public async Task CreateTeacherAsync_ShouldThrow_WhenUserIsNotTeacher()
        {
            await using var context = CreateDbContext();

            var identity = new TeacherUserResponseDto
            {
                Id = "user-123",
                Email = "student@test.com",
                Role = "Student"
            };

            _identityServiceClientMock.Setup(x => x.GetUserByIdAsync("user-123")).ReturnsAsync(identity);

            var request = new CreateTeacherProfileRequestDto
            {
                UserId = "user-123",
                EmployeeCode = "T001",
                FirstName = "John",
                LastName = "Mathew",
                Phone = "9876543210"
            };

            var service  = new TeacherServiceClass(_identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                            _auditServiceClientMock.Object,
                            context,
                            _loggerMock.Object);


            Assert.ThrowsAsync<BadRequestException>(() => service.CreateTeacherAsync(request, "Admin1"));
        }


        [Fact]
        public async Task GetMyProfileAsync_ShouldReturnProfile_WhenTeacherExists()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                UserId = "user-123",
                EmployeeCode = "T001",
                FirstName = "John",
                LastName = "Mathew",
                Phone = "9876543210",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow
            };

            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = new TeacherServiceClass(_identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                           _auditServiceClientMock.Object,
                           context,
                           _loggerMock.Object);


            var result = await service.GetMyProfileAsync("user-123");

            Assert.NotNull(result);
            Assert.Equal("user-123", result.UserId);
            Assert.Equal("T001", result.EmployeeCode);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Mathew", result.LastName);
            Assert.Equal("9876543210", result.Phone);
            Assert.True(result.IsActive);
        }


        [Fact]
        public async Task UpdateMyProfileAsync_ShouldUpdateProfile_WhenTeacherExists()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                UserId = "user-123",
                EmployeeCode = "T001",
                FirstName = "John",
                LastName = "Mathew",
                Phone = "9876543210",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow
            };

            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var request = new UpdateTeacherProfileRequestDto
            {
                FirstName = "David",
                LastName = "Thomas",
                Phone = "9999999999"
            };

            var service = new TeacherServiceClass(_identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                            _auditServiceClientMock.Object,
                            context,
                            _loggerMock.Object);

            var result = await service.UpdateMyProfileAsync(request, "user-123");

            Assert.NotNull(result);
            Assert.Equal("David", result.FirstName);
            Assert.Equal("Thomas", result.LastName);
            Assert.Equal("9999999999", result.Phone);

            var updatedTeacher = await context.Teachers.FirstOrDefaultAsync(x => x.UserId == "user-123");

            Assert.NotNull(updatedTeacher);
            Assert.Equal("David", updatedTeacher.FirstName);
            Assert.Equal("Thomas", updatedTeacher.LastName);
            Assert.Equal("9999999999", updatedTeacher.Phone);
        }


        [Fact]
        public async Task UploadProfilePhotoAsync_ShouldUpdatePhoto_WhenTeacherExists()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                UserId = "user-123",
                EmployeeCode = "T001",
                FirstName = "John",
                LastName = "Mathew",
                Phone = "9876543210",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow
            };

            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var photoUrl = "/uploads/teachers/profile-123.jpg";

            var service = new TeacherServiceClass(_identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                           _auditServiceClientMock.Object,
                           context,
                           _loggerMock.Object);

            var result = await service.UploadProfilePhotoAsync(photoUrl, "user-123");

            Assert.NotNull(result);
            Assert.Equal(photoUrl, result.ProfilePhotoUrl);

            var updatedTeacher = await context.Teachers.FirstOrDefaultAsync(x => x.UserId == "user-123");

            Assert.NotNull(updatedTeacher);
            Assert.Equal(photoUrl, updatedTeacher.ProfilePhotoUrl);
        }
    }
}
