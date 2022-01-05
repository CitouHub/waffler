using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using NSubstitute;
using Xunit;

using Waffler.Service;
using Waffler.Test.Helper;
using Waffler.Test.Mock;
using Waffler.Domain.Github;

namespace Waffler.Test.Service
{
    public class GithubServiceTest
    {
        private readonly ILogger<GithubService> _logger = Substitute.For<ILogger<GithubService>>();
        private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
        private readonly string ApiBaseUri = "https://test.api.com";
        private readonly string Owner = "Test";

        private GithubService _githubService;

        [Fact]
        public async Task GetLatestRelease_NoReleases()
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Github")).Returns(httpClient);
            var settings = new Dictionary<string, string> {
                {"Github:Owner", Owner}
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _githubService = new GithubService(configuration, _logger, _httpClientFactory);

            //Act
            var result = await _githubService.GetLatestReleaseAsync();

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/repos/{Owner}/waffler/releases", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Fact]
        public async Task GetLatestRelease_EmptyList()
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(new List<ReleaseDTO>()), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Github")).Returns(httpClient);
            var settings = new Dictionary<string, string> {
                {"Github:Owner", Owner}
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _githubService = new GithubService(configuration, _logger, _httpClientFactory);

            //Act
            var result = await _githubService.GetLatestReleaseAsync();

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/repos/{Owner}/waffler/releases", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Fact]
        public async Task GetLatestRelease_Releases()
        {
            //Setup
            var release1 = GithubHelper.GetRelease();
            var release2 = GithubHelper.GetRelease();
            var release3 = GithubHelper.GetRelease();
            release1.created_at = new DateTime(2022, 1, 1, 12, 0, 0);
            release1.name = "v1.0";
            release2.created_at = new DateTime(2022, 1, 1, 17, 0, 0);
            release2.name = "v3.0";
            release3.created_at = new DateTime(2022, 1, 1, 14, 0, 0);
            release3.name = "v2.0";
            var releases = new List<ReleaseDTO>() { release1, release2, release3 };
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(releases), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Github")).Returns(httpClient);
            var settings = new Dictionary<string, string> {
                {"Github:Owner", Owner}
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _githubService = new GithubService(configuration, _logger, _httpClientFactory);

            //Act
            var result = await _githubService.GetLatestReleaseAsync();

            //Assert
            Assert.Equal("v3.0", result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/repos/{Owner}/waffler/releases", httpMessageHandler.Requests[0].RequestUri.ToString());
        }
    }
}
