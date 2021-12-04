using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Waffler.Common;
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
        Task<List<OrderDTO>> GetOrdersAsync(string instrumentCode, DateTime from, DateTime to);
        Task<OrderDTO> GetOrderAsync(Guid orderId);
    }

    public class BitpandaService : IBitpandaService
    {
        private readonly IConfiguration _configuration;
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

        public BitpandaService(
            IConfiguration configuration, 
            ILogger<BitpandaService> logger, 
            WafflerDbContext context, 
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("Bitpanda");
            _apiKey = context.WafflerProfiles.FirstOrDefault()?.ApiKey;
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

            //TODO: WHY is the Z included in some cases and in some not?!
            //This fix solves the problem for now...
            fromString = fromString.EndsWith("Z") ? fromString : fromString + "Z";
            toString = toString.EndsWith("Z") ? toString : toString + "Z";

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

        public async Task<OrderSubmittedDTO> CreateOrderAsync(CreateOrderDTO order)
        {
            if(PrivateHttpClient != null && 
                (order.Side == Bitpanda.Side.BUY && _configuration.GetValue<bool>("Bitpanda:OrderFeature:Buy") == true) ||
                (order.Side == Bitpanda.Side.SELL && _configuration.GetValue<bool>("Bitpanda:OrderFeature:Sell") == true))
            {
                    var requestContent = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                    var result = await PublicHttpClient.SendAsync(new HttpRequestMessage()
                    {
                        RequestUri = new Uri($"account/orders"),
                        Method = HttpMethod.Post,
                        Content = requestContent,
                    });
                    var content = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OrderSubmittedDTO>(content);                
            }

            return null;
        }

        public async Task<List<OrderDTO>> GetOrdersAsync(string instrumentCode, DateTime from, DateTime to)
        {
            if (PrivateHttpClient != null)
            {
                var fromString = HttpUtility.UrlEncode(from.ToString("o"));
                var toString = HttpUtility.UrlEncode(to.ToString("o"));

                //TODO: WHY is the Z included in some cases and in some not?!
                //This fix solves the problem for now...
                fromString = fromString.EndsWith("Z") ? fromString : fromString + "Z";
                toString = toString.EndsWith("Z") ? toString : toString + "Z";

                var activeOrders = await PrivateHttpClient.GetAsync($"account/orders/?" +
                    $"from={fromString}&" +
                    $"to={toString}&" +
                    $"instrumentCode={instrumentCode}");
                var finishedOrder = await PrivateHttpClient.GetAsync($"account/orders/?" +
                    $"from={fromString}&" +
                    $"to={toString}&" +
                    $"instrumentCode={instrumentCode}&" +
                    $"with_just_filled_inactive=true");
                var aborterOrder = await PrivateHttpClient.GetAsync($"account/orders/?" +
                    $"from={fromString}&" +
                    $"to={toString}&" +
                    $"instrumentCode={instrumentCode}&" +
                    $"with_cancelled_and_rejected=true");

                var orders = new List<OrderDTO>();

                if (activeOrders.IsSuccessStatusCode)
                {
                    var content = await activeOrders.Content.ReadAsStringAsync();
                    var orderHistory = JsonConvert.DeserializeObject<OrderHistoryDTO>(content);
                    orders.AddRange(orderHistory?.Order_history?.Select(_ => _.Order));
                }

                if (finishedOrder.IsSuccessStatusCode)
                {
                    var content = await finishedOrder.Content.ReadAsStringAsync();
                    var orderHistory = JsonConvert.DeserializeObject<OrderHistoryDTO>(content);
                    orders.AddRange(orderHistory?.Order_history?.Select(_ => _.Order).Where(_ => orders.Any(o => o.Order_id == _.Order_id) == false));
                }

                if (aborterOrder.IsSuccessStatusCode)
                {
                    var content = await aborterOrder.Content.ReadAsStringAsync();
                    var orderHistory = JsonConvert.DeserializeObject<OrderHistoryDTO>(content);
                    orders.AddRange(orderHistory?.Order_history?.Select(_ => _.Order).Where(_ => orders.Any(o => o.Order_id == _.Order_id) == false));
                }

                return orders;
            }

            return null;
        }

        public async Task<OrderDTO> GetOrderAsync(Guid orderId)
        {
            if (PrivateHttpClient != null)
            {
                var result = await PrivateHttpClient.GetAsync($"account/orders/{orderId}");

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OrderHistoryEntityDTO>(content)?.Order;
                }
            }

            return null;
        }
    }
}
