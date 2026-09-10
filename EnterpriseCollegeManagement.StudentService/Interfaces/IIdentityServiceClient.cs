using EnterpriseCollegeManagement.StudentService.DTOs.Responses;

namespace EnterpriseCollegeManagement.StudentService.Interfaces
{
    public interface IIdentityServiceClient
    {
        //get data from identitydb

        Task<IdentityUserResponseDto?> GetUserAsync(string userId);  //may get null value 
    }
}
