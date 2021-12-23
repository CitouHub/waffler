using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Waffler.Common;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Test.Helper;
using Xunit;

namespace Waffler.Test
{
    public class TradeServiceTest
    {
        private readonly ICandleStickService _candleStickService = Substitute.For<ICandleStickService>();
        private readonly ITradeRuleService _tradeRuleService = Substitute.For<ITradeRuleService>();
        private readonly ITradeOrderService _tradeoOrderService = Substitute.For<ITradeOrderService>();
        private readonly IBitpandaService _bitpandaService = Substitute.For<IBitpandaService>();
        private readonly TradeService _tradeService;

        private readonly int TestTradeRuleId = 1;

        public TradeServiceTest()
        {
            var logger = Substitute.For<ILogger<TradeService>>();

            _tradeService = new TradeService(logger, _candleStickService, _tradeRuleService, _tradeoOrderService, _bitpandaService);
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_TradeRuleStatusInactive()
        {
            //Setup
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeRuleStatusId = (short)Variable.TradeRuleStatus.Inactive;
            tradeRule.LastTrigger = DateTime.UtcNow.AddDays(-1).AddMinutes(-1);
            tradeRule.TradeMinIntervalMinutes = 24 * 60;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(TestTradeRuleId, DateTime.UtcNow);

            //Assert
            Assert.Null(result);
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_TriggerInterval()
        {
            //Setup
            var tradeRule = TradeRuleHelper.GetTradeRule();
            tradeRule.TradeRuleStatusId = (short)Variable.TradeRuleStatus.Active;
            tradeRule.LastTrigger = DateTime.UtcNow.AddDays(-1).AddMinutes(1);
            tradeRule.TradeMinIntervalMinutes = 24 * 60;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(TestTradeRuleId, DateTime.UtcNow);

            //Assert
            Assert.Null(result);
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_NoConditions()
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRule = TradeRuleHelper.GetTradeRule();
            var candleStick = CandleStickHelper.GetCandleStick();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(TestTradeRuleId, currentPeriodDateTime);

            //Assert
            Assert.Null(result);
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
        }
    }
}
