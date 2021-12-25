using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Waffler.Common;
using Waffler.Data;
using Waffler.Service;
using Waffler.Test.Helper;
using Waffler.Test.Mock;
using Xunit;

namespace Waffler.Test.Service
{
    public class BitpandaServiceTest
    {
        private readonly ILogger<BitpandaService> _logger = Substitute.For<ILogger<BitpandaService>>();
        private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
        private readonly MockHttpMessageHandler _httpMessageHandler = new(null, HttpStatusCode.OK);
        
        private readonly Mock<WafflerDbContext> _context;
        private BitpandaService _bitpandaService;

        public BitpandaServiceTest()
        {
            _context = new Mock<WafflerDbContext>();
            var httpClient = new HttpClient(_httpMessageHandler);
            _httpClientFactory.CreateClient(Arg.Is("Bitpanda")).Returns(httpClient);
        }

        [Fact]
        public async Task PlaceOrderAsync_NoProfile()
        {
            //Setup
            var profiles = new List<WafflerProfile>().AsQueryable();
            _context.Setup(m => m.WafflerProfiles).Returns(DatabaseHelper.GetMockDbSet(profiles).Object);
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", "true"},
                {"Bitpanda:OrderFeature:Sell", "true"},
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, _context.Object, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, 10, 10);

            //Assert
            Assert.Null(result);
            Assert.Equal(0, _httpMessageHandler.NumberOfCalls);
        }

        [Theory]
        [InlineData((short)Variable.TradeAction.Buy, false, true)]
        [InlineData((short)Variable.TradeAction.Buy, false, false)]
        [InlineData((short)Variable.TradeAction.Sell, true, false)]
        [InlineData((short)Variable.TradeAction.Sell, false, false)]
        public async Task PlaceOrderAsync_ActionDisabled(short tradeActionId, bool buy, bool sell)
        {
            //Setup
            var profiles = new List<WafflerProfile>
            {
                ProfileHelper.GetProfile()
            }.AsQueryable();
            _context.Setup(m => m.WafflerProfiles).Returns(DatabaseHelper.GetMockDbSet(profiles).Object);
            var settings = new Dictionary<string, string> {
                {"Bitpanda:OrderFeature:Buy", buy ? "true" : "false"},
                {"Bitpanda:OrderFeature:Sell", sell ? "true" : "false"},
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            _bitpandaService = new BitpandaService(configuration, _logger, _context.Object, _httpClientFactory);
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeActionId = tradeActionId;

            //Act
            var result = await _bitpandaService.PlaceOrderAsync(tradeRule, 10, 10);

            //Assert
            Assert.Null(result);
            Assert.Equal(0, _httpMessageHandler.NumberOfCalls);
        }
    }
}
