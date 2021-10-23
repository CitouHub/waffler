using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Waffler.Data;
using Waffler.Domain.Bitpanda.Private;
using Waffler.Domain.Bitpanda.Private.Balance;
using Waffler.Domain.Bitpanda.Private.Order;
using Waffler.Domain.Bitpanda.Public;

namespace Waffler.Service
{
    public interface IBitpandaService
    {
        Task<AccountDTO> GetAccountAsync();
        Task<List<CandleStickDTO>> GetCandleSticks(string instrumentCode, string unit, short period, DateTime from, DateTime to);
        Task<OrderSubmittedDTO> CreateOrderAsync(CreateOrderDTO createOrder);
        Task GetOrderAsync();
    }

    public class BitpandaService : IBitpandaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private HttpClient? PrivateHttpClient 
        { 
            get {
                if(string.IsNullOrEmpty(_apiKey) == false)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                    return _httpClient;
                }
                return null;
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

        public BitpandaService(WafflerDbContext context, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Bitpanda");
            _apiKey = context.WafflerProfile.FirstOrDefault()?.ApiKey;
        }

        public async Task<AccountDTO> GetAccountAsync()
        {
            if(PrivateHttpClient != null)
            {
                var result = await PrivateHttpClient.GetAsync("account/balances");
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AccountDTO>(content);
            }

            return null;
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

            if(result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CandleStickDTO>>(content);
            } 
            else
            {
                return null;
            }
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
