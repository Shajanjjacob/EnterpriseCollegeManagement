using AutoMapper;
using EnterpriseCollegeManagement.StudentService.DTOs.Requests;
using EnterpriseCollegeManagement.StudentService.DTOs.Responses;
using EnterpriseCollegeManagement.StudentService.Entities;

namespace EnterpriseCollegeManagement.StudentService.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateStudentProfileRequestDto, Student>();

            CreateMap<Student, StudentResponseDto>()
            .ForMember(
                dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department.Name));



                



        }
    }
}
