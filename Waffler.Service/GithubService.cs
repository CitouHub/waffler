using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Waffler.Domain.Github;

namespace Waffler.Service
{
    public interface IGithubService
    {
        Task<string> GetLatestReleaseAsync();
    }

    public class GithubService : IGithubService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GithubService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string Owner;

        public GithubService(
            IConfiguration configuration,
            ILogger<GithubService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("Github");
            Owner = _configuration.GetValue<string>("Github:Owner");
            _logger.LogDebug("GithubService instantiated");
        }

        public async Task<string> GetLatestReleaseAsync()
        {
            var result = await _httpClient.GetAsync($"repos/{Owner}/waffler/releases");
            var content = await result.Content.ReadAsStringAsync();
            var releases = JsonConvert.DeserializeObject<List<ReleaseDTO>>(content);

            return releases?.OrderByDescending(_ => _.created_at)?.FirstOrDefault()?.name;
        }
    }
}
