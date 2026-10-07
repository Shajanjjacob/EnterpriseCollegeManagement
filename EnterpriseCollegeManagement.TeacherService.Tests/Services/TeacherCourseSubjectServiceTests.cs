using EnterpriseCollegeManagement.TeacherService.Data;
using EnterpriseCollegeManagement.TeacherService.DTOs.Requests;
using EnterpriseCollegeManagement.TeacherService.DTOs.Responses;
using EnterpriseCollegeManagement.TeacherService.Entities;
using EnterpriseCollegeManagement.TeacherService.Exceptions;
using EnterpriseCollegeManagement.TeacherService.Interfaces;
using EnterpriseCollegeManagement.TeacherService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseCollegeManagement.TeacherService.Tests.Services
{
    public  class TeacherCourseSubjectServiceTests
    {

        private readonly Mock<IStudentServiceClient> _studentServiceClientMock;
        private readonly Mock<IAcademicServiceClient> _academicServiceClientMock;
        private readonly Mock<IIdentityServiceClient> _identityServiceClientMock;
        private readonly Mock<IAuditServiceClient> _auditServiceClientMock;
        private readonly Mock<ILogger<TeacherCourseSubjectService>> _loggerMock;

        public TeacherCourseSubjectServiceTests()
        {
            _academicServiceClientMock = new Mock<IAcademicServiceClient>();
            _identityServiceClientMock = new Mock<IIdentityServiceClient>();
            _auditServiceClientMock = new Mock<IAuditServiceClient>();
            _studentServiceClientMock = new Mock<IStudentServiceClient>();
            _loggerMock = new Mock<ILogger<TeacherCourseSubjectService>>();

        }

        private TeacherDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<TeacherDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new TeacherDbContext(options);
        }

        [Fact]
        public async Task AssignCourseSubjectAsync_ShouldAssign_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                
                Id = 1,
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

            var courseSubject = new CourseSubjectResponseDto
            {
                Id = 10,
                CourseId = 1,
                CourseName = "Computer Science",
                SubjectId = 5,
                SubjectName = "Programming in C#",
                Semester = 3
            };

            var course = new CourseResponseDto
            {
                Id = 1,
                Code = "CSE",
                Name = "Computer Science",
                DepartmentId = 2,
                DepartmentName = "Computer Science"
            };

            var department = new DepartmentResponse
            {
                Id = 2,
                Name = "Computer Science"
            };

            _academicServiceClientMock.Setup(x => x.GetCourseSubjectByIdAsync(10)).ReturnsAsync(courseSubject);
            _academicServiceClientMock.Setup(x => x.GetCourseByIdAsync(1)).ReturnsAsync(course);
            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(2)).ReturnsAsync(department);

            var service = new TeacherCourseSubjectService(_auditServiceClientMock.Object,_identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                          context,
                          _loggerMock.Object);

            var request = new AssignTeacherCourseSubjectRequestDto
            {
                TeacherId = teacher.Id,
                CourseSubjectId = 10
            };


            var result = service.AssignCourseSubjectAsync(request, "admin-123");

            Assert.NotNull(result);
            Assert.Equal(teacher.Id, result.Id);


            var assignment = await context.TeacherCourseSubjects.FirstOrDefaultAsync(x =>  x.TeacherId == teacher.Id && x.CourseSubjectId == 10);

            Assert.NotNull(assignment);
            Assert.False(assignment.IsDeleted);
        }

        [Fact]
        public async Task AssignCourseSubjectAsync_ShouldThrow_WhenAssignmentAlreadyExists()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                Id = 1,
                EmployeeCode = "T001",
                FirstName = "John",
                LastName = "Mathew",
                Phone = "9876543210",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow
            };

            context.Teachers.Add(teacher);

            var existingAssignment = new TeacherCourseSubject
            {
                TeacherId = teacher.Id,
                CourseSubjectId = 10,
                AssignedDate = DateTime.UtcNow,
                CreatedBy = "admin-123",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.TeacherCourseSubjects.Add(existingAssignment);

            await context.SaveChangesAsync();

            var request = new AssignTeacherCourseSubjectRequestDto
            {
                TeacherId = teacher.Id,
                CourseSubjectId = 10
            };

            var service = new TeacherCourseSubjectService(_auditServiceClientMock.Object, _identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                          context,
                          _loggerMock.Object);

            await Assert.ThrowsAsync<ConflictException>(() => service.AssignCourseSubjectAsync(request, "admin-123"));
        }


        [Fact]
        public async Task GetAssignedCourseSubjectsAsync_ShouldReturnAssignments()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                Id = 1,
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

            var assignment = new TeacherCourseSubject
            {
                TeacherId = teacher.Id,
                CourseSubjectId = 10,
                AssignedDate = DateTime.UtcNow,
                CreatedBy = "admin-123",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.TeacherCourseSubjects.Add(assignment);
            await context.SaveChangesAsync();

            var courseSubject = new CourseSubjectResponseDto
            {
                Id = 10,
                CourseId = 1,
                CourseName = "Computer Science",
                SubjectId = 5,
                SubjectName = "Programming in C#",
                Semester = 3
            };

            var course = new CourseResponseDto
            {
                Id = 1,
                Code = "CSE",
                Name = "Computer Science",
                DepartmentId = 2,
                DepartmentName = "Computer Science"
            };

            var department = new DepartmentResponse
            {
                Id = 2,
                Name = "Computer Science"
            };

            _academicServiceClientMock.Setup(x => x.GetCourseSubjectByIdAsync(10)).ReturnsAsync(courseSubject);

            _academicServiceClientMock.Setup(x => x.GetCourseByIdAsync(1)).ReturnsAsync(course);

            _studentServiceClientMock.Setup(x => x.GetDepartmentByIdAsync(2)).ReturnsAsync(department);

            var service = new TeacherCourseSubjectService(_auditServiceClientMock.Object, _identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                          context,
                          _loggerMock.Object);

            var result = await service.GetAssignedCourseSubjectsAsync(teacher.Id);

            Assert.NotNull(result);
           

            Assert.Equal(teacher.Id, result[0].TeacherId);
            Assert.Equal(10, result[0].CourseSubjectId);
            Assert.Equal(1, result[0].CourseId);
            Assert.Equal("Computer Science", result[0].CourseName);
            Assert.Equal(5, result[0].SubjectId);
            Assert.Equal("Programming in C#", result[0].SubjectName);
            Assert.Equal(3, result[0].Semester);
            Assert.Equal(2, result[0].DepartmentId);
            Assert.Equal("Computer Science", result[0].DepartmentName);
        }


        [Fact]
        public async Task RemoveCourseSubjectAsync_ShouldSoftDeleteAssignment()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                Id = 1,
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

            var assignment = new TeacherCourseSubject
            {
                TeacherId = teacher.Id,
                CourseSubjectId = 10,
                AssignedDate = DateTime.UtcNow,
                CreatedBy = "admin-123",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            context.TeacherCourseSubjects.Add(assignment);
            await context.SaveChangesAsync();

            var courseSubject = new CourseSubjectResponseDto
            {
                Id = 10,
                CourseId = 1,
                CourseName = "Computer Science",
                SubjectId = 5,
                SubjectName = "Programming in C#",
                Semester = 3
            };

            _academicServiceClientMock.Setup(x => x.GetCourseSubjectByIdAsync(10)).ReturnsAsync(courseSubject);

            var service = new TeacherCourseSubjectService(_auditServiceClientMock.Object, _identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                         context,
                         _loggerMock.Object);

            await service.RemoveCourseSubjectAsync(teacher.Id, 10,"admin-123");

            var removedAssignment = await context.TeacherCourseSubjects.FirstOrDefaultAsync(x => x.Id == assignment.Id);

            Assert.NotNull(removedAssignment);
            Assert.True(removedAssignment.IsDeleted);
            Assert.Equal("admin-123", removedAssignment.DeletedBy);
            Assert.NotNull(removedAssignment.DeletedDate);
        }


        [Fact]
        public async Task AssignCourseSubjectAsync_ShouldThrow_WhenCourseSubjectDoesNotExist()
        {
            await using var context = CreateDbContext();

            var teacher = new Teacher
            {
                UserId = "teacher-user-123",
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

            _academicServiceClientMock
                .Setup(x => x.GetCourseSubjectByIdAsync(10))
                .ReturnsAsync((CourseSubjectResponseDto?)null);

            var request = new AssignTeacherCourseSubjectRequestDto
            {
                TeacherId = teacher.Id,
                CourseSubjectId = 10
            };

            var service = new TeacherCourseSubjectService(_auditServiceClientMock.Object, _identityServiceClientMock.Object, _academicServiceClientMock.Object, _studentServiceClientMock.Object,
                         context,
                         _loggerMock.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => service.AssignCourseSubjectAsync( request, "admin-123"));
        }
    }

}
