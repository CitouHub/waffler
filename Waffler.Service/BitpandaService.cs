using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Waffler.Common;
using Waffler.Data;
using Waffler.Domain;
using Waffler.Domain.Bitpanda.Private.Balance;
using Waffler.Domain.Bitpanda.Private.Order;
using Waffler.Domain.Converter;

namespace Waffler.Service
{
    public interface IBitpandaService
    {
        Task<AccountDTO> GetAccountAsync();
        Task<List<Domain.Bitpanda.Public.CandleStickDTO>> GetCandleSticksAsync(string instrumentCode, string unit, short period, DateTime from, DateTime to);
        Task<OrderSubmittedDTO> TryPlaceOrderAsync(TradeRuleDTO tradeRule, decimal amount, decimal price);
        Task<List<OrderDTO>> GetOrdersAsync(string instrumentCode, DateTime from, DateTime to);
        Task<OrderDTO> GetOrderAsync(Guid orderId);
    }

    public class BitpandaService : IBitpandaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BitpandaService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private HttpClient PrivateHttpClient
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey) == false)
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
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("Bitpanda");
            _apiKey = context.WafflerProfiles.OrderBy(_ => _.Id)?.FirstOrDefault()?.ApiKey;
            _logger.LogDebug("Instantiated");
        }

        public async Task<AccountDTO> GetAccountAsync()
        {
            if (PrivateHttpClient != null)
            {
                var result = await PrivateHttpClient.GetAsync("account/balances");
                if(result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AccountDTO>(content);
                }
            }

            return null;
        }

        public async Task<List<Domain.Bitpanda.Public.CandleStickDTO>> GetCandleSticksAsync(string instrumentCode, string unit, short period, DateTime from, DateTime to)
        {
            var fromString = DateTimeStringFormatConverter.GetDateTimeString(from);
            var toString = DateTimeStringFormatConverter.GetDateTimeString(to);

            var result = await PublicHttpClient.GetAsync($"candlesticks/{instrumentCode}?" +
                $"unit={unit}&" +
                $"period={period}&" +
                $"from={fromString}&" +
                $"to={toString}");

            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Domain.Bitpanda.Public.CandleStickDTO>>(content);
            }
            else
            {
                return null;
            }
        }

        private async Task<OrderSubmittedDTO> PlaceOrderAsync(CreateOrderDTO order)
        {
            var orderJson = JsonConvert.SerializeObject(order, new DecimalStringFormatConverter());
            var requestContent = new StringContent(orderJson, Encoding.UTF8, "application/json");

            var result = await PrivateHttpClient.PostAsync($"account/orders", requestContent);

            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OrderSubmittedDTO>(content);
            }
            else
            {
                var error = await result.Content.ReadAsStringAsync();
                _logger.LogError($"Unable to place order, {error}");
            }

            return null;
        }

        public async Task<OrderSubmittedDTO> TryPlaceOrderAsync(TradeRuleDTO tradeRule, decimal amount, decimal price)
        {
            if (PrivateHttpClient != null &&
                (tradeRule.TradeActionId == (short)Variable.TradeAction.Buy && _configuration.GetValue<bool>("Bitpanda:OrderFeature:Buy") == true) ||
                (tradeRule.TradeActionId == (short)Variable.TradeAction.Sell && _configuration.GetValue<bool>("Bitpanda:OrderFeature:Sell") == true))
            {
                var balance = await GetAccountAsync();
                var buyBalance = balance?.Balances?.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.EUR);
                var sellBalance = balance?.Balances?.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.BTC);

                if ((buyBalance?.Available >= amount * price && tradeRule.TradeActionId == (short)Variable.TradeAction.Buy) ||
                    (sellBalance?.Available >= amount * price && tradeRule.TradeActionId == (short)Variable.TradeAction.Sell))
                {
                    var order = new CreateOrderDTO()
                    {
                        Amount = amount,
                        Type = Bitpanda.OrderType.LIMIT,
                        Expire_after = tradeRule.TradeOrderExpirationMinutes != null ? DateTime.UtcNow.AddMinutes((int)tradeRule.TradeOrderExpirationMinutes) : (DateTime?)null,
                        Instrument_code = Bitpanda.GetInstrumentCode((Variable.TradeType)tradeRule.TradeTypeId),
                        Price = price,
                        Side = Bitpanda.GetSide((Variable.TradeAction)tradeRule.TradeActionId),
                        Time_in_force = tradeRule.TradeOrderExpirationMinutes != null ? Bitpanda.TimeInForce.GOOD_TILL_TIME : Bitpanda.TimeInForce.GOOD_TILL_CANCELLED
                    };

                    return await PlaceOrderAsync(order);
                }
                else
                {
                    _logger.LogWarning($"Unable to place order for rule {tradeRule.Name}, insufficient balance, EUR: {buyBalance?.Available}, BTC: {sellBalance?.Available}");
                }
            }
            else
            {
                _logger.LogWarning($"Unable to place order for rule {tradeRule.Name}, trade action is disabled of API-key is missing");
            }

            return null;
        }

        public async Task<List<OrderDTO>> GetOrdersAsync(string instrumentCode, DateTime from, DateTime to)
        {
            if (PrivateHttpClient != null)
            {
                var fromString = DateTimeStringFormatConverter.GetDateTimeString(from);
                var toString = DateTimeStringFormatConverter.GetDateTimeString(to);

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

                if(activeOrders.IsSuccessStatusCode || activeOrders.IsSuccessStatusCode || activeOrders.IsSuccessStatusCode)
                {
                    return orders;
                }
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
