using ElasticSearch.Models;

namespace ElasticSearch.Services
{
    public interface IElasticService
    {
        Task CreateIndexAsync(string indexName);
        Task DeleteIndexAsync(string indexName);
        Task<bool> CreateOrUpdateUser(User user);
        Task<bool> AddOrUpdateBulk(IEnumerable<User> users, string indexName);
        Task<User> GetUser(string Key);
        Task<IEnumerable<User>>? GetAll();
        Task<bool> Remove(string key);
        Task<long?> RemoveAll();

        Task<IEnumerable<User>> GetUsersWithFuzzySearchOnMultipleFields(string key);
        Task<IEnumerable<User>> GetUsersWithFuzzySearch(string key);

    }
}
