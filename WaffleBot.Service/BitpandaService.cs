using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WaffleBot.Data;
using WaffleBot.Domain.Bitpanda.Private;
using WaffleBot.Domain.Bitpanda.Private.Order;
using WaffleBot.Domain.Bitpanda.Public;

namespace WaffleBot.Service
{
    public interface IBitpandaService
    {
        Task<string> GetBalanceAsync();
        Task<List<CandleStickDTO>> GetCandleSticks(string instrumentCode, string unit, short period, DateTime from, DateTime to);
        Task<OrderSubmittedDTO> CreateOrderAsync(CreateOrderDTO createOrder);
        Task GetOrderAsync();
    }

    public class BitpandaService : IBitpandaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private HttpClient PrivateHttpClient 
        { 
            get {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                return _httpClient;
            } 
        }

        private HttpClient PublicHttpClient
        {
            get
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return _httpClient;
            }
        }

        public BitpandaService(IHttpClientFactory httpClientFactory, IProfileService profileService)
        {
            _httpClient = httpClientFactory.CreateClient("Bitpanda");
            _apiKey = profileService.GetApiKey().Result;
        }

        public async Task<string> GetBalanceAsync()
        {
            var result = await PrivateHttpClient.GetAsync("account/balances");
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<List<CandleStickDTO>> GetCandleSticks(string instrumentCode, string unit, short period, DateTime from, DateTime to)
        {
            var fromString = HttpUtility.UrlEncode(from.ToString("o"));
            var toString = HttpUtility.UrlEncode(to.ToString("o"));

            var result = await PublicHttpClient.GetAsync($"candlesticks/{instrumentCode}?" +
                $"unit={unit}&" +
                $"period={period}&" +
                $"from={fromString}&" +
                $"to={toString}");
            var content = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<CandleStickDTO>>(content);
        }

        public async Task<OrderSubmittedDTO> CreateOrderAsync(CreateOrderDTO createOrder)
        {
            var requestContent = new StringContent(JsonConvert.SerializeObject(createOrder), Encoding.UTF8, "application/json");
            var result = await PublicHttpClient.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri($"account/orders"),
                Method = HttpMethod.Post,
                Content = requestContent,
            });
            var content = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OrderSubmittedDTO>(content);
        }

        public Task GetOrderAsync()
        {
            throw new NotImplementedException();
        }
    }
}
