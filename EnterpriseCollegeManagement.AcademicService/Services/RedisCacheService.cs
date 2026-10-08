using EnterpriseCollegeManagement.AcademicService.Interfaces;
using Microsoft.OpenApi.Writers;
using StackExchange.Redis;

namespace EnterpriseCollegeManagement.AcademicService.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)  //(IConnectionMultiplexer ConnectionMultiplexer)
        {
            _database = redis.GetDatabase();
        }

        //The key => is the name/identifier of the data stored in Redis. 
        //store and retrive data for the particular key 

        //The value=>  is the actual data we want to store. (dtos then convert into json)

        //TimeSpan expiration => How long should this cached data remain available?(w need time ,if later changed
        //something and not give time span old value will show so need time span 

        public async Task<string?> GetAsync(string key)  //get something from redis 
        {
            return await _database.StringGetAsync(key);
        }

        public async Task RemoveAsync(string key)   //delete something from redis
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task SetAsync(string key, string value, TimeSpan expiration)  // strore something in redis 
        {
            await _database.StringSetAsync( key,value,  expiration);
        }
    }
}
