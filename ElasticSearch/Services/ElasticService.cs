
using Elasticsearch.Net;
using ElasticSearch.Helpers;
using ElasticSearch.Models;
using Microsoft.Extensions.Options;
using Nest;

namespace ElasticSearch.Services
{
    public class ElasticService : IElasticService
    {
        private readonly ElasticClient _client;
        private readonly ElasticSetting elasticSetting;
        public ElasticService(IOptions<ElasticSetting> options)
        {
            this.elasticSetting = options.Value;

            var settings = new ConnectionSettings(new Uri(elasticSetting.Url))
                .DefaultIndex(elasticSetting.DefaultIndex);

            this._client = new ElasticClient(settings);
        }
        //no
        public async Task<bool> AddOrUpdateBulk(IEnumerable<User> users, string indexName)
        {
            var response = await _client.BulkAsync(b => b.Index(elasticSetting.DefaultIndex)
            .UpdateMany(users, (ud, u) => ud.Doc(u).DocAsUpsert(true)));

            if (!response.IsValid)
            {
                // Handle errors - you can log them or throw an exception based on your needs
                Console.WriteLine($"Bulk indexing failed: {response.OriginalException?.Message}");
                foreach (var item in response.ItemsWithErrors)
                {
                    Console.WriteLine($"Failed operation: {item.Error.Reason}");
                }
            }

            return response.IsValid;
        }

        public async Task CreateIndexAsync(string indexName)
        {
            if (!_client.Indices.Exists(indexName).Exists)
                await _client.Indices.CreateAsync(indexName);
        }

        public async Task<bool> CreateOrUpdateUser(User user)
        {
            var response = await _client.IndexAsync(user, index =>
                        index.Index(elasticSetting.DefaultIndex)
                        .OpType(OpType.Index));

            return response.IsValid;
        }
        // no
        public Task DeleteIndexAsync(string indexName)
        {
            throw new NotImplementedException();
        }
        //no
        public async Task<IEnumerable<User>?> GetAll()
        {
            var response = await _client.SearchAsync<User>(s =>
          s.Index(elasticSetting.DefaultIndex));

            return response.IsValid ? response.Documents.ToList() : default;
        }

        public async Task<User> GetUser(string Key)
        {
            var response = await _client.GetAsync<User>(Key, g =>
                         g.Index(elasticSetting.DefaultIndex));
            var searchResponse = _client.Search<User>(s => s
                       .Query(q => q
                           .MultiMatch(mm => mm
                               .Query(Key)  // The search term or key to query
                               .Type(TextQueryType.BestFields)  // Match the best field
                           )));
            return response.Source;
        }
        //no
        public async Task<bool> Remove(string key)
        {
            var response = await _client.DeleteAsync<User>(key, d =>
            d.Index(elasticSetting.DefaultIndex));

            return response.IsValid;
        }
        //no
        public async Task<long?> RemoveAll()
        {
            var response = await _client.DeleteByQueryAsync<User>(d => d.Index(elasticSetting.DefaultIndex));
            return response.IsValid ? response.Deleted : default;

        }

        public async Task<IEnumerable<User>> GetUsersWithFuzzySearch(string key)
        {
            var fuzzySearchResponse = _client.Search<User>(s => s
                      .Query(q => q
                          .Match(m => m
                              .Field(f => f.FirstName)  // Specify the field to search
                              .Query(key)  // The term to search for with fuzzy matching
                              .Fuzziness(Fuzziness.EditDistance(2)) // Use a specific fuzziness value, e.g., 2
                          )
                      )
                  );

            return fuzzySearchResponse.Hits.Select(hit => hit.Source);
        }
        public async Task<IEnumerable<User>> GetUsersWithFuzzySearchOnMultipleFields(string key)
        {
            var fuzzySearchResponseWithMultipleFields = _client.Search<User>(s => s
                           .Query(q => q
                               .MultiMatch(mm => mm
                                   .Fields(f => f
                                       .Field(u => u.FirstName)
                                       .Field(u => u.LastName)  // Add more fields as needed
                                   )
                                   .Query(key)  // The term to search for with fuzzy matching
                                   .Fuzziness(Fuzziness.EditDistance(2)) // Use a specific fuzziness value, e.g., 2
                               )
                           )
                        );

            /*
            search term => joh

            result =>   {
                    "id": 6,
                    "firstName": "Jean",
                    "lastName": "Doe"
                    }

            [Deo] match [joh] with Fuzziness distance 2 the character o match in both
             */

            return fuzzySearchResponseWithMultipleFields.Hits.Select(hit => hit.Source);
        }
    }
}
