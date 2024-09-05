using ElasticSearch.Models;
using ElasticSearch.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElasticController : ControllerBase
    {
        private readonly IElasticService elasticService;

        public ElasticController(IElasticService elasticService)
        {
            this.elasticService = elasticService;
        }

        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            await elasticService.CreateIndexAsync(indexName);
            return Ok("Index Created");
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var res = await elasticService.CreateOrUpdateUser(user);
            if (res)
                return Ok("User Created");

            return BadRequest();
        }

        [HttpGet("get-user/{key}")]
        public async Task<IActionResult> GetUser(string key)
        {
            var user = await elasticService.GetUser(key);
            if (user == null)
                return BadRequest("User not found");
            else
                return Ok(user);

        }


        // Get user action with a proper route and HTTP method
        [HttpGet("get-users-with-fuzzy-search/{key}")]
        public async Task<IActionResult> GetUsersWithFuzzySearch(string key)
        {
            var users = await elasticService.GetUsersWithFuzzySearch(key);
            if (users == null)
                return NoContent();
            else
                return Ok(users);

        }

        [HttpGet("get-users-with-fuzzy-search-multiple-fields/{key}")]
        public async Task<IActionResult> GetUsersWithFuzzySearchOnMultipleFields(string key)
        {
            var users = await elasticService.GetUsersWithFuzzySearchOnMultipleFields(key);
            if (users == null)
                return NoContent();
            else
                return Ok(users);
        }

        [HttpPost("index-bulk")]
        public async Task<IActionResult> IndexBulk(IEnumerable<User> users, string indexName)
        {
            var result = await elasticService.AddOrUpdateBulk(users, indexName);

            if (result)
                return Ok("Bulk Added Successfully");
            else
                return BadRequest();
        }

    }
}
