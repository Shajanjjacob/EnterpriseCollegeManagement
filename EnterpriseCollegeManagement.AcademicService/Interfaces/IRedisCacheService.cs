namespace EnterpriseCollegeManagement.AcademicService.Interfaces
{
    public interface IRedisCacheService
    {
        Task<string?> GetAsync(string key);   //Redis stores values as strings. We have  serialize our DTOs to json

        Task SetAsync(string key,string value,TimeSpan expiration);

        Task RemoveAsync(string key);
    }
}
