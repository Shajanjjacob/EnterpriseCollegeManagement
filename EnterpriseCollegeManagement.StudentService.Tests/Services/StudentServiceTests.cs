using AutoMapper;
using Azure.Core;
using EnterpriseCollegeManagement.StudentService.Data;
using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Entities;
using EnterpriseCollegeManagement.StudentService.Exceptions;
using EnterpriseCollegeManagement.StudentService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using StudentServiceClass =
    EnterpriseCollegeManagement.StudentService.Services.StudentService;

namespace EnterpriseCollegeManagement.StudentService.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<StudentServiceClass>> _loggerMock;
        private readonly Mock<IIdentityServiceClient> _identityClientMock;


        public StudentServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<StudentServiceClass>>();
            _identityClientMock = new Mock<IIdentityServiceClient>();
        }

        private StudentDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<StudentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            return new StudentDbContext(options);
        }

        [Fact]

        public async Task CreateStudentAsync_ShouldCreateStudent_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var department = new Department
            {
                Id = 1,
                Code = "CSE",
                Name = "Computer Science and Engineering"
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var request = new CreateStudentProfileRequestDto
            {
                UserId = "user-001",
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun",
                LastName = "Menon",
                DateOfBirth = new DateTime(2002, 5, 15),
                Phone = "9876543210",
                Address = "Kerala",
                DepartmentId = 1,
                EnrollmentDate = new DateTime(2026, 6, 10)

            };

            var identityUser = new IdentityUserResponseDto
            {
                UserId = "user-001",
                Email = "arjun@example.com",
                Role = "Student"
            };

            var student = new Student
            {
                UserId = request.UserId,
                AdmissionNumber = request.AdmissionNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Phone = request.Phone,
                Address = request.Address,
                DepartmentId = request.DepartmentId,
                EnrollmentDate = request.EnrollmentDate
            };

            var expectedResponse = new StudentResponseDto
            {
                Id = 1,
                UserId = "user-001",
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun",
                LastName = "Menon",
                DepartmentId = 1,
                DepartmentName = "Computer Science and Engineering"
            };

            _identityClientMock.Setup(x => x.GetUserAsync(request.UserId)).ReturnsAsync(identityUser);

            _mapperMock.Setup(x => x.Map<Student>(request)).Returns(student);

            _mapperMock.Setup(x => x.Map<StudentResponseDto>(It.IsAny<Student>())).Returns(expectedResponse);

            var service = new StudentServiceClass(
              context,
              _mapperMock.Object,
              _loggerMock.Object,
              _identityClientMock.Object
             );
            var result = await service.CreateStudentAsync( request,"admin-001"); //object and actoruserid

            // verify studentresponse 
            Assert.NotNull(result);
            Assert.Equal("user-001", result.UserId);
            Assert.Equal("ECM2026001", result.AdmissionNumber);
            Assert.Equal("Arjun", result.FirstName);
            Assert.Equal(1, result.DepartmentId);

            //get data from database 
            var savedStudent = await context.Students.FirstOrDefaultAsync(x => x.UserId == "user-001");

            // verify return data from db
            Assert.NotNull(savedStudent);
            Assert.Equal("admin-001", savedStudent.CreatedBy);
            Assert.False(savedStudent.IsDeleted);


        }

        [Fact]

        public async Task CreateStudentAsync_ShouldThrowException_WhenAdmissionNumberAlreadyExists()        {
            await using var context = CreateDbContext();

            var department = new Department
            {
                Id = 1,
                Name = "Computer Science",
                Code = "CSE"
            };

            context.Departments.Add(department);

            var existingStudent = new Student  //mock existing one then check with same admission no again 
            {
                Id = 1,
                UserId = "user-001",
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun",
                LastName = "Student",
                DepartmentId = 1,
                IsDeleted = false
            };
            context.Students.Add(existingStudent);
            await context.SaveChangesAsync();

            var request = new CreateStudentProfileRequestDto
            {
                UserId = "user-002",
                AdmissionNumber = "ECM2026001",
                FirstName = "Rahul",
                LastName = "Student",
                DepartmentId = 1
            };

            _identityClientMock.Setup(x => x.GetUserAsync("user-002"))
                .ReturnsAsync(new IdentityUserResponseDto
                {
                    UserId = "user-002",
                    Role = "Student"

                });

            var service = new StudentServiceClass(
                        context,
                        _mapperMock.Object,
                        _loggerMock.Object,
                        _identityClientMock.Object
                    );

            await Assert.ThrowsAsync<ConflictException>(

                () => service.CreateStudentAsync(request, "admin-001")
                );

        }

        [Fact]
        public async Task CreateStudentAsync_ShouldThrowException_WhenDepartmentDoesNotExist()
        {
            await using var context = CreateDbContext();

            var request = new CreateStudentProfileRequestDto
            {
                UserId = "user-003",
                AdmissionNumber = "ECM2026002",
                FirstName = "Rahul",
                LastName = "Menon",
                DateOfBirth = new DateTime(2002, 5, 15),
                Phone = "9876543210",
                Address = "Kerala",
                DepartmentId = 99,   // Invalid department
                EnrollmentDate = new DateTime(2026, 6, 10)
            };

            _identityClientMock.Setup(x => x.GetUserAsync("user-003"))
                .ReturnsAsync(new IdentityUserResponseDto
                {
                    UserId = "user-003",
                    Email = "rahul@example.com",
                    Role = "Student"
                });


            var service = new StudentServiceClass(
                        context,
                        _mapperMock.Object,
                        _loggerMock.Object,
                        _identityClientMock.Object
                    );

            await Assert.ThrowsAsync<NotFoundException>(
                () => service.CreateStudentAsync(request, "admin-001")

                );

        }

        [Fact]

        public async Task CreateStudentAsync_ShouldThrowException_WhenIdentityUserDoesNotExist()
        {
            await using var context = CreateDbContext();

            var request = new CreateStudentProfileRequestDto
            {
                UserId = "user-004",
                AdmissionNumber = "ECM2026003",
                FirstName = "Vishnu",
                LastName = "Kumar",
                DateOfBirth = new DateTime(2002, 5, 15),
                Phone = "9876543210",
                Address = "Kerala",
                DepartmentId = 1,
                EnrollmentDate = new DateTime(2026, 6, 10)
            };


            _identityClientMock.Setup(x => x.GetUserAsync("user-004"))
                .ReturnsAsync((IdentityUserResponseDto?) null);

            var service = new StudentServiceClass(
                       context,
                       _mapperMock.Object,
                       _loggerMock.Object,
                       _identityClientMock.Object
                   );

            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateStudentAsync(request, "admin-001"));


        }

        [Fact]
        public async Task CreateStudentAsync_ShouldThrowException_WhenIdentityUserIsNotStudent()
        {
            await using var context = CreateDbContext();

            var request = new CreateStudentProfileRequestDto
            {
                UserId = "user-005",
                AdmissionNumber = "ECM2026004",
                FirstName = "Anil",
                LastName = "Kumar",
                DateOfBirth = new DateTime(2002, 5, 15),
                Phone = "9876543210",
                Address = "Kerala",
                DepartmentId = 1,
                EnrollmentDate = new DateTime(2026, 6, 10)
            };


            _identityClientMock.Setup(x => x.GetUserAsync("user-005"))
                .ReturnsAsync(new IdentityUserResponseDto
                {
                    UserId = "user-005",
                    Email = "anil@example.com",
                    Role = "Teacher"

                });

            var service = new StudentServiceClass(
                        context,
                        _mapperMock.Object,
                        _loggerMock.Object,
                        _identityClientMock.Object
                    );

            await Assert.ThrowsAsync<BadRequestException>(
       () => service.CreateStudentAsync(request, "admin-001")); 
        }

        [Fact]

        public async Task CreateStudentAsync_ShouldThrowException_WhenStudentProfileAlreadyExists()
        {
            await using var context = CreateDbContext();

            var department = new Department
            {
                Id = 1,
                Name = "Computer Science",
                Code = "CSE"
            };
            context.Departments.Add(department);

            var existingStudent = new Student
            {
                Id = 1,
                UserId = "user-001",
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun",
                LastName = "Menon",
                DepartmentId = 1,
                IsDeleted = false

            };

            context.Students.Add(existingStudent);
            await context.SaveChangesAsync();

            var request = new CreateStudentProfileRequestDto
            {
                UserId = "user-001",              // SAME UserId
                AdmissionNumber = "ECM2026002",  // Different admission number
                FirstName = "Arjun",
                LastName = "Menon",
                DepartmentId = 1
            };

            _identityClientMock.Setup(x => x.GetUserAsync("user-001"))
                .ReturnsAsync(new IdentityUserResponseDto
                {
                    UserId = "user-001",
                    Email = "arjun@example.com",
                    Role = "Student"
                });

            var service = new StudentServiceClass(
                    context,
                    _mapperMock.Object,
                    _loggerMock.Object,
                    _identityClientMock.Object
                );

            await Assert.ThrowsAsync<ConflictException>(
       () => service.CreateStudentAsync(request, "admin-001"));
        }


        [Fact]

        public async Task UpdateStudentAsync_ShouldUpdateStudent_WhenRequestIsValid()
        {
            await using var context  = CreateDbContext();

            var department = new Department
            {
                Id = 1,
                Name = "Computer Science",
                Code = "CSE"
            };

            context.Departments.Add(department);

            var existingstudent = new Student
            {
                Id = 1,
                UserId = "user-001",
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun",
                LastName = "Menon",
                DateOfBirth = new DateTime(2002, 5, 15),
                Phone = "9876543210",
                Address = "Kerala",
                DepartmentId = 1,
                EnrollmentDate = new DateTime(2026, 6, 10),
                IsDeleted = false,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow
            };

            context.Students.Add(existingstudent);
            await context.SaveChangesAsync();

            var request = new AdminUpdateStudentRequestDto
            {
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun Updated",
                LastName = "Menon",
                DateOfBirth = new DateTime(2002, 5, 15),
                Phone = "9999999999",
                Address = "Kochi, Kerala", //change
                DepartmentId = 1,
                EnrollmentDate = new DateTime(2026, 6, 10)
            };

            var expectedresponse = new StudentResponseDto
            {
                Id = 1,
                UserId = "user-001",
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun Updated",
                LastName = "Menon",
                DepartmentId = 1,
                DepartmentName = "Computer Science"
            };

            _mapperMock.Setup(x => x.Map<StudentResponseDto>(It.IsAny<Student>()))
                .Returns(expectedresponse);

            var service = new StudentServiceClass(
                           context,
                           _mapperMock.Object,
                           _loggerMock.Object,
                           _identityClientMock.Object
                       );

            var result = await service.UpdateStudentAsync(1, request, "admin-002");
         //result response testing 

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("ECM2026001", result.AdmissionNumber);
            Assert.Equal("Arjun Updated", result.FirstName);
            Assert.Equal("Menon", result.LastName);
            Assert.Equal(1, result.DepartmentId);

           
            var updatedStudent = await context.Students.FirstOrDefaultAsync(x => x.Id == 1);

            
            Assert.NotNull(updatedStudent);
            Assert.Equal("Arjun Updated", updatedStudent.FirstName);
            Assert.Equal("9999999999", updatedStudent.Phone);
            Assert.Equal("Kochi, Kerala", updatedStudent.Address);
            Assert.Equal("admin-002", updatedStudent.UpdatedBy);
            Assert.NotNull(updatedStudent.UpdatedDate);
            Assert.False(updatedStudent.IsDeleted);
        }


        [Fact]

        public async Task DeleteStudentAsync_ShouldSoftDeleteStudent_WhenStudentExists()
        {
            await using var context = CreateDbContext();

            var student = new Student
            {
                Id = 1,
                UserId = "user-001",
                AdmissionNumber = "ECM2026001",
                FirstName = "Arjun",
                LastName = "Menon",
                DepartmentId = 1,
                IsDeleted = false

            };

            context.Students.Add(student);
            await context.SaveChangesAsync();

            var service = new StudentServiceClass(
                           context,
                           _mapperMock.Object,
                           _loggerMock.Object,
                           _identityClientMock.Object
                       );

           
            var result = await service.DeleteStudentAsync(1,"admin-002");

           
            Assert.True(result);

           
            var deletedStudent = await context.Students.FirstOrDefaultAsync(x => x.Id == 1);

            
            Assert.NotNull(deletedStudent);

           
            Assert.True(deletedStudent.IsDeleted);
            Assert.Equal("admin-002", deletedStudent.DeletedBy);
            Assert.NotNull(deletedStudent.DeletedDate);
        }

    }

}
