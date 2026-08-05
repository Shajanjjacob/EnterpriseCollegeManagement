using AutoMapper;
using EnterpriseCollegeManagement.IdentityService.DTOs.Auth.Requests;
using EnterpriseCollegeManagement.IdentityService.Entities;

namespace EnterpriseCollegeManagement.IdentityService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequestDto, ApplicationUser>();
        }
    }
}
