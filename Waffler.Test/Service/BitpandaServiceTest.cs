using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Threading.Tasks;
using Waffler.Common;
using Waffler.Data;
using Waffler.Domain.Bitpanda.Private.Balance;
using Waffler.Service;
using Waffler.Test.Helper;
using Waffler.Test.Mock;
using Xunit;

#pragma warning disable IDE0017 // Simplify object initialization
namespace Waffler.Test.Service
{
    public class BitpandaServiceTest
    {
        private readonly ILogger<BitpandaService> _logger = Substitute.For<ILogger<BitpandaService>>();
        private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
        private readonly string ApiBaseUri = "https://test.api.com";

        private BitpandaService _bitpandaService;

        [Fact]
        public async Task GetAccountAsync_NoProfile()
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetAccountAsync();

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Fact]
        public async Task GetAccountAsync_NoApiKey()
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            var profile = ProfileHelper.GetProfile();
            profile.ApiKey = null;
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetAccountAsync();

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Fact]
        public async Task GetAccountAsync_BadRequest()
        {
            //Setup
            var account = BitpandaHelper.GetAccount();
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(account), HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetAccountAsync();

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/account/balances", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Fact]
        public async Task GetAccountAsync_OK()
        {
            //Setup
            var account = BitpandaHelper.GetAccount();
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(account), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetAccountAsync();

            //Assert
            Assert.NotNull(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/account/balances", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Theory]
        [InlineData(0, Bitpanda.InstrumentCode.BTC_EUR, Bitpanda.Period.MINUTES, 15, "2020-01-01 12:00", "2020-01-01 18:00")]
        public async Task GetCandleSticksAsync_BadRequest(short nbrCandleSticks, string instrumentCode, string unit, short period, DateTime from, DateTime to)
        {
            //Setup
            var candleSticks = BitpandaHelper.GetCandleSticks(nbrCandleSticks);
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(candleSticks), HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetCandleSticksAsync(instrumentCode, unit, period, from, to);

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Contains($"{ApiBaseUri}/candlesticks/{instrumentCode}?unit={unit}&period={period}", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Theory]
        [InlineData(0, Bitpanda.InstrumentCode.BTC_EUR, Bitpanda.Period.MINUTES, 15, "2020-01-01 12:00", "2020-01-01 18:00")]
        [InlineData(1, Bitpanda.InstrumentCode.BTC_EUR, Bitpanda.Period.MINUTES, 15, "2020-01-01 12:00", "2020-01-01 18:00")]
        [InlineData(50, Bitpanda.InstrumentCode.BTC_EUR, Bitpanda.Period.MINUTES, 15, "2020-01-01 12:00", "2020-01-01 18:00")]
        public async Task GetCandleSticksAsync_OK(short nbrCandleSticks, string instrumentCode, string unit, short period, DateTime from, DateTime to)
        {
            //Setup
            var candleSticks = BitpandaHelper.GetCandleSticks(nbrCandleSticks);
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(candleSticks), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetCandleSticksAsync(instrumentCode, unit, period, from, to);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(nbrCandleSticks, result.Count);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Contains($"{ApiBaseUri}/candlesticks/{instrumentCode}?unit={unit}&period={period}", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Fact]
        public async Task PlaceOrderAsync_NoOrder_NoProfile()
        {
            //Setup
            var  httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", "true"},
                {"Bitpanda:OrderFeature:Sell", "true"},
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, context, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, 10, 10);

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Fact]
        public async Task PlaceOrderAsync_NoOrder_NoApiKey()
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            var profile = ProfileHelper.GetProfile();
            profile.ApiKey = null;
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", "true"},
                {"Bitpanda:OrderFeature:Sell", "true"},
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, context, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, 10, 10);

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Theory]
        [InlineData((short)Variable.TradeAction.Buy, false, true)]
        [InlineData((short)Variable.TradeAction.Buy, false, false)]
        [InlineData((short)Variable.TradeAction.Sell, true, false)]
        [InlineData((short)Variable.TradeAction.Sell, false, false)]
        public async Task PlaceOrderAsync_NoOrder_ActionDisabled(short tradeActionId, bool buy, bool sell)
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", buy ? "true" : "false"},
                {"Bitpanda:OrderFeature:Sell", sell ? "true" : "false"},
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, context, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeActionId = tradeActionId;

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, 10, 10);

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Theory]
        [InlineData((short)Variable.TradeAction.Buy)]
        [InlineData((short)Variable.TradeAction.Sell)]
        public async Task PlaceOrderAsync_NoOrder_NoBalance(short tradeActionId)
        {
            //Setup
            var account = BitpandaHelper.GetAccount();
            account.Balances = new List<BalanceDTO>();
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(account), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", "true" },
                {"Bitpanda:OrderFeature:Sell", "true" },
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, context, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeActionId = tradeActionId;

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, 10, 10);

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/account/balances", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Theory]
        [InlineData((short)Variable.TradeAction.Buy, 0.1, 100, 9, 11)]
        [InlineData((short)Variable.TradeAction.Sell, 0.1, 100, 11, 9)]
        public async Task PlaceOrderAsync_NoOrder_InsufficientBalance(short tradeActionId, decimal amount, decimal price, decimal euro, decimal btc)
        {
            //Setup
            var account = BitpandaHelper.GetAccount();
            var euroBalance = account.Balances.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.EUR);
            euroBalance.Available = euro;
            var btcBalance = account.Balances.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.BTC);
            btcBalance.Available = btc;
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(account), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", "true" },
                {"Bitpanda:OrderFeature:Sell", "true" },
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, context, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeActionId = tradeActionId;

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, amount, price);

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/account/balances", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Theory]
        [InlineData((short)Variable.TradeAction.Buy, 0.1, 100, 11, 0)]
        public async Task PlaceOrderAsync_Order_BadRequest(short tradeActionId, decimal amount, decimal price, decimal euro, decimal btc)
        {
            //Setup
            var account = BitpandaHelper.GetAccount();
            var euroBalance = account.Balances.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.EUR);
            euroBalance.Available = euro;
            var btcBalance = account.Balances.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.BTC);
            btcBalance.Available = btc;
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(account), HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", "true" },
                {"Bitpanda:OrderFeature:Sell", "true" },
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, context, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeActionId = tradeActionId;

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, amount, price);

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/account/balances", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Theory]
        [InlineData((short)Variable.TradeAction.Buy, 0.1, 100, 11, 0)]
        [InlineData((short)Variable.TradeAction.Buy, 0.1, 100, 11, 11)]
        [InlineData((short)Variable.TradeAction.Sell, 0.1, 100, 0, 11)]
        [InlineData((short)Variable.TradeAction.Sell, 0.1, 100, 11, 11)]
        public async Task PlaceOrderAsync_Order_OK(short tradeActionId, decimal amount, decimal price, decimal euro, decimal btc)
        {
            //Setup
            var account = BitpandaHelper.GetAccount();
            var euroBalance = account.Balances.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.EUR);
            euroBalance.Available = euro;
            var btcBalance = account.Balances.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.BTC);
            btcBalance.Available = btc;
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(account), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", "true" },
                {"Bitpanda:OrderFeature:Sell", "true" },
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, context, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeActionId = tradeActionId;

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, amount, price);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, httpMessageHandler.Requests.Count);
            Assert.Equal($"{ApiBaseUri}/account/balances", httpMessageHandler.Requests[0].RequestUri.ToString());
            Assert.Equal($"{ApiBaseUri}/account/orders", httpMessageHandler.Requests[1].RequestUri.ToString());
        }

        [Theory]
        [InlineData(Bitpanda.InstrumentCode.BTC_EUR, "2020-01-01 12:00", "2020-01-01 18:00")]
        public async Task GetOrdersAsync_NoProfile(string instrumentCode, DateTime from, DateTime to)
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrdersAsync(instrumentCode, from, to);

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Theory]
        [InlineData(Bitpanda.InstrumentCode.BTC_EUR, "2020-01-01 12:00", "2020-01-01 18:00")]
        public async Task GetOrdersAsync_NoApiKey(string instrumentCode, DateTime from, DateTime to)
        {
            //Setup
            var httpMessageHandler = new MockHttpMessageHandler(null, HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            var profile = ProfileHelper.GetProfile();
            profile.ApiKey = null;
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrdersAsync(instrumentCode, from, to);

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Theory]
        [InlineData(0, Bitpanda.InstrumentCode.BTC_EUR, "2020-01-01 12:00", "2020-01-01 18:00")]
        public async Task GetOrdersAsync_BadRequest(short nbrOrders, string instrumentCode, DateTime from, DateTime to)
        {
            //Setup
            var orders = BitpandaHelper.GetOrders(nbrOrders);
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(orders), HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrdersAsync(instrumentCode, from, to);

            //Assert
            Assert.Null(result);
            Assert.Contains($"{ApiBaseUri}/account/orders", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Theory]
        [InlineData(0, Bitpanda.InstrumentCode.BTC_EUR, "2020-01-01 12:00", "2020-01-01 18:00")]
        [InlineData(1, Bitpanda.InstrumentCode.BTC_EUR, "2020-01-01 12:00", "2020-01-01 18:00")]
        [InlineData(50, Bitpanda.InstrumentCode.BTC_EUR, "2020-01-01 12:00", "2020-01-01 18:00")]
        public async Task GetOrdersAsync_OK(short nbrOrders, string instrumentCode, DateTime from, DateTime to)
        {
            //Setup
            var orders = BitpandaHelper.GetOrders(nbrOrders);
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(orders), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrdersAsync(instrumentCode, from, to);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(nbrOrders, result.Count);
            Assert.Equal(3, httpMessageHandler.Requests.Count);
            Assert.Contains($"{ApiBaseUri}/account/orders", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Fact]
        public async Task GetOrderAsync_NoProfile()
        {
            //Setup
            var order = BitpandaHelper.GetOrder();
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(order), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrderAsync(new Guid(order.Order.Order_id));

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Fact]
        public async Task GetOrderAsync_NoApiKey()
        {
            //Setup
            var order = BitpandaHelper.GetOrder();
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(order), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            var profile = ProfileHelper.GetProfile();
            profile.ApiKey = null;
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrderAsync(new Guid(order.Order.Order_id));

            //Assert
            Assert.Null(result);
            Assert.Empty(httpMessageHandler.Requests);
        }

        [Fact]
        public async Task GetOrderAsync_BadRequest()
        {
            //Setup
            var order = BitpandaHelper.GetOrder();
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(order), HttpStatusCode.BadRequest);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrderAsync(new Guid(order.Order.Order_id));

            //Assert
            Assert.Null(result);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/account/orders/{order.Order.Order_id}", httpMessageHandler.Requests[0].RequestUri.ToString());
        }

        [Fact]
        public async Task GetOrderAsync_OK()
        {
            //Setup
            var order = BitpandaHelper.GetOrder();
            var httpMessageHandler = new MockHttpMessageHandler(JsonConvert.SerializeObject(order), HttpStatusCode.OK);
            var httpClient = new HttpClient(httpMessageHandler);
            httpClient.BaseAddress = new Uri(ApiBaseUri);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
            using var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            _bitpandaService = new BitpandaService(Substitute.For<IConfiguration>(), _logger, context, _httpClientFactory);

            //Act
            var result = await _bitpandaService.GetOrderAsync(new Guid(order.Order.Order_id));

            //Assert
            Assert.NotNull(result);
            Assert.Equal(order.Order.Order_id, result.Order_id);
            Assert.Single(httpMessageHandler.Requests);
            Assert.Equal($"{ApiBaseUri}/account/orders/{order.Order.Order_id}", httpMessageHandler.Requests[0].RequestUri.ToString());
        }
    }
}
