using AutoMapper;
using Castle.Core.Logging;
using EnterpriseCollegeManagement.StudentService.Data;
using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Entities;
using EnterpriseCollegeManagement.StudentService.Exceptions;
using EnterpriseCollegeManagement.StudentService.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseCollegeManagement.StudentService.Tests.Services
{
    public class DepartmentServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<Microsoft.Extensions.Logging.ILogger<DepartmentService>> _loggerMock;


        public DepartmentServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<DepartmentService>>();

        }

        private StudentDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<StudentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new StudentDbContext(options);
        }

        [Fact]
        public async Task CreateDepartmentAsync_ShouldCreateDepartment_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();


            var request = new CreateDepartmentRequestDto
            {
                Code = "cse",
                Name = "Computer Science and Engineering"
            };

            var expectedresponse = new DepartmentResponseDto
            {
                Id = 1,
                Code = "CSE",
                Name = "Computer Science and Engineering"
            };

            _mapperMock.Setup(x => x.Map<DepartmentResponseDto>(It.IsAny<Department>())).Returns(expectedresponse);

            var service = new DepartmentService(context, _mapperMock.Object, _loggerMock.Object);

            var result = await service.CreateDepartmentAsync(request,"admin-001");

            

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("CSE", result.Code);
            Assert.Equal("Computer Science and Engineering",result.Name);

            
            var savedDepartment = await context.Departments.FirstOrDefaultAsync(x => x.Code == "CSE");

           

            Assert.NotNull(savedDepartment);
            Assert.Equal("Computer Science and Engineering",savedDepartment.Name);
            Assert.Equal("CSE", savedDepartment.Code);
            Assert.Equal("admin-001", savedDepartment.CreatedBy);
            Assert.False(savedDepartment.IsDeleted);

        }

        [Fact]
        public async Task GetDepartmentByIdAsync_ShouldReturnDepartment_WhenDepartmentExists()
        {
            await using var context = CreateDbContext();

            var department = new Department
            {
                Name = "Computer Science and Engineering",
                Code = "CSE"
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            var expectedresponse = new DepartmentResponseDto
            {
                Id = 1,
                Name = "Computer Science and Engineering",
                Code = "CSE"
            };

            _mapperMock.Setup(x => x.Map<DepartmentResponseDto>(It.IsAny<Department>()))
                .Returns(expectedresponse);


            var service = new DepartmentService(context, _mapperMock.Object,_loggerMock.Object);

            var result = await service.GetDepartmentByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("CSE", result.Code);
            Assert.Equal("Computer Science and Engineering",result.Name);
        }

        [Fact] 
        public async Task UpdateDepartmentAsync_ShouldUpdateDepartment_WhenRequestIsValid()
        {
            await using var context = CreateDbContext();

            var existingdepartment = new Department
            {
                Id = 1,
                Name = "Computer Science",
                Code = "CSE",
                IsDeleted = false,
                CreatedBy = "admin-001",
                CreatedDate = DateTime.UtcNow
            };

            context.Departments.Add(existingdepartment);
            await context.SaveChangesAsync();

            var request = new UpdateDepartmentRequestDto
            {
                Name = "Information Technology",
                Code = "IT"
            };

            var expectedresponse = new DepartmentResponseDto
            {
                Id = 1,
                Name = "Information Technology",
                Code = "IT"
            };

            _mapperMock.Setup(x => x.Map<DepartmentResponseDto>(It.IsAny<Department>()))
                .Returns(expectedresponse);

            var service = new DepartmentService(context, _mapperMock.Object, _loggerMock.Object);

            var result = await service.UpdateDepartmentAsync(request, "admin-002", 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("IT", result.Code);
            Assert.Equal("Information Technology", result.Name);

          
            var updatedDepartment = await context.Departments.FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(updatedDepartment);

            Assert.Equal("Information Technology",updatedDepartment.Name);

            Assert.Equal("IT", updatedDepartment.Code);

            Assert.Equal( "admin-002",updatedDepartment.UpdatedBy);

            Assert.NotNull(updatedDepartment.UpdatedDate);

            Assert.False(updatedDepartment.IsDeleted);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_ShouldThrowException_WhenActiveStudentsAreAssigned()
        {
            await using var context = CreateDbContext();


            var department = new Department
            {
                Id = 1,
                Name = "Computer Science",
                Code = "CSE",
                IsDeleted = false

            };
            context.Departments.Add(department);
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

            var service = new DepartmentService( context,_mapperMock.Object,_loggerMock.Object);

      
            await Assert.ThrowsAsync<ConflictException>(() => service.DeleteDepartmentAsync(1, "admin-002"));

            //department still exists or not 
            var savedDepartment = await context.Departments .FirstOrDefaultAsync(x => x.Id == 1);

            Assert.NotNull(savedDepartment);

           
            Assert.False(savedDepartment.IsDeleted);

        }
    }
}
