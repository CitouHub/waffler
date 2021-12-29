using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waffler.Common;
using Waffler.Common.Util;
using Waffler.Data;
using Waffler.Data.ComplexModel;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Test.Helper;
using Xunit;

#pragma warning disable IDE0017 // Simplify object initialization
namespace Waffler.Test.Service
{
    public class StatisticsServiceTest
    {
        private readonly ILogger<StatisticsService> _logger = Substitute.For<ILogger<StatisticsService>>();
        private readonly ICandleStickService _candleStickService = Substitute.For<ICandleStickService>();
        private readonly Mock<WafflerDbContext> _context = new Mock<WafflerDbContext>();
        private readonly StatisticsService _statisticsService;

        public StatisticsServiceTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            var _mapper = mapperConfig.CreateMapper();

            _statisticsService = new StatisticsService(_logger, _context.Object, _mapper, _candleStickService);
        }

        [Theory]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, "2021-01-01 12:30", 5, "2021-01-01 12:28", "2021-01-01 12:32")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, "2021-01-01 12:30", 120, "2021-01-01 11:30", "2021-01-01 13:30")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, "2021-01-01 12:30", 1505, "2020-12-31 23:58", "2021-01-02 01:02")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, "2021-01-01 12:30", 5, "2021-01-01 12:25", "2021-01-01 12:30")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, "2021-01-01 12:30", 120, "2021-01-01 10:30", "2021-01-01 12:30")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, "2021-01-01 12:30", 1505, "2020-12-31 11:25", "2021-01-01 12:30")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, "2021-01-01 12:30", 5, "2021-01-01 12:30", "2021-01-01 12:35")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, "2021-01-01 12:30", 120, "2021-01-01 12:30", "2021-01-01 14:30")]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, "2021-01-01 12:30", 1505, "2021-01-01 12:30", "2021-01-02 13:35")]
        public void GetPeriod(Variable.TradeRuleConditionPeriodDirection tradeRuleConditionPeriodDirectionId, DateTime date, int periodMinutes, DateTime expectedFrom, DateTime expectedTo)
        {
            //Act
            var period = _statisticsService.GetPeriod(tradeRuleConditionPeriodDirectionId, date, periodMinutes);

            //Assert
            Assert.Equal(expectedFrom, period.From);
            Assert.Equal(expectedTo, period.To);
        }

        [Theory]
        [InlineData(Variable.CandleStickValueType.HighPrice, 100)]
        [InlineData(Variable.CandleStickValueType.OpenPrice, 100)]
        [InlineData(Variable.CandleStickValueType.ClosePrice, 100)]
        [InlineData(Variable.CandleStickValueType.LowPrice, 100)]
        public void GetPrice(Variable.CandleStickValueType candleStickValueType, int expectedPrice)
        {
            //Setup
            var candleStick = CandleStickHelper.GetCandleStick();
            candleStick.HighPrice = candleStickValueType == Variable.CandleStickValueType.HighPrice ? expectedPrice : 0;
            candleStick.OpenPrice = candleStickValueType == Variable.CandleStickValueType.OpenPrice ? expectedPrice : 0;
            candleStick.ClosePrice = candleStickValueType == Variable.CandleStickValueType.ClosePrice ? expectedPrice : 0;
            candleStick.LowPrice = candleStickValueType == Variable.CandleStickValueType.LowPrice ? expectedPrice : 0;

            //Act
            var price = _statisticsService.GetPrice(candleStickValueType, candleStick);

            //Assert
            Assert.Equal(expectedPrice, price);
        }

        [Theory]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, 100, Variable.TradeRuleConditionPeriodDirection.Centered, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, 100, Variable.TradeRuleConditionPeriodDirection.Centered, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, 100, Variable.TradeRuleConditionPeriodDirection.LeftShift, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, 100, Variable.TradeRuleConditionPeriodDirection.LeftShift, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, 100, Variable.TradeRuleConditionPeriodDirection.RightShift, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.Centered, 100, Variable.TradeRuleConditionPeriodDirection.RightShift, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, 100, Variable.TradeRuleConditionPeriodDirection.Centered, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, 100, Variable.TradeRuleConditionPeriodDirection.Centered, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, 100, Variable.TradeRuleConditionPeriodDirection.LeftShift, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, 100, Variable.TradeRuleConditionPeriodDirection.LeftShift, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, 100, Variable.TradeRuleConditionPeriodDirection.RightShift, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.LeftShift, 100, Variable.TradeRuleConditionPeriodDirection.RightShift, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, 100, Variable.TradeRuleConditionPeriodDirection.Centered, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, 100, Variable.TradeRuleConditionPeriodDirection.Centered, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, 100, Variable.TradeRuleConditionPeriodDirection.LeftShift, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, 100, Variable.TradeRuleConditionPeriodDirection.LeftShift, 90, -10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, 100, Variable.TradeRuleConditionPeriodDirection.RightShift, 110, 10.0)]
        [InlineData(Variable.TradeRuleConditionPeriodDirection.RightShift, 100, Variable.TradeRuleConditionPeriodDirection.RightShift, 90, -10.0)]
        public async Task GetPriceTrendAsync(Variable.TradeRuleConditionPeriodDirection fromTradeRuleConditionPeriodDirection, int fromPrice,
            Variable.TradeRuleConditionPeriodDirection toTradeRuleConditionPeriodDirection, int toPrice, decimal expetedTrend)
        {
            //Setup
            var from = new DateTime(2021, 1, 1, 12, 0, 0);
            var to = from.AddDays(1);
            var fromPeriodMinutes = 60;
            var toPeriodMinutes = 60;
            var fromPeriod = _statisticsService.GetPeriod(fromTradeRuleConditionPeriodDirection, from, fromPeriodMinutes);
            var toPeriod = _statisticsService.GetPeriod(toTradeRuleConditionPeriodDirection, to, toPeriodMinutes);
            var fromCandleSticks = GetCandleSticks(Variable.CandleStickValueType.HighPrice, fromPrice, fromPeriod);
            var toCandleSticks = GetCandleSticks(Variable.CandleStickValueType.HighPrice, toPrice, toPeriod);

            _candleStickService.GetCandleSticksAsync(Arg.Is(fromPeriod.From), Arg.Is(fromPeriod.To), Arg.Is(Variable.TradeType.BTC_EUR), Arg.Is(fromPeriodMinutes)).Returns(fromCandleSticks);
            _candleStickService.GetCandleSticksAsync(Arg.Is(toPeriod.From), Arg.Is(toPeriod.To), Arg.Is(Variable.TradeType.BTC_EUR), Arg.Is(toPeriodMinutes)).Returns(toCandleSticks);

            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleCondition();
            tradeRuleCondition.FromCandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice;
            tradeRuleCondition.FromMinutes = -1 * (int)(to - from).TotalMinutes;
            tradeRuleCondition.FromPeriodMinutes = fromPeriodMinutes;
            tradeRuleCondition.FromTradeRuleConditionPeriodDirectionId = (short)fromTradeRuleConditionPeriodDirection;
            tradeRuleCondition.ToCandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice;
            tradeRuleCondition.ToMinutes = 0;
            tradeRuleCondition.ToPeriodMinutes = toPeriodMinutes;
            tradeRuleCondition.ToTradeRuleConditionPeriodDirectionId = (short)toTradeRuleConditionPeriodDirection;

            //Act
            var trend = await _statisticsService.GetPriceTrendAsync(to, Variable.TradeType.BTC_EUR, tradeRuleCondition);

            //Assert
            Assert.Equal(expetedTrend, trend.Change);
        }

        private List<CandleStickDTO> GetCandleSticks(Variable.CandleStickValueType candleStickValueType, int avgPrice, PeriodDTO period)
        {
            var candleSticks = Enumerable.Repeat(CandleStickHelper.GetCandleStick(), 5).ToList();
            var periodMinutes = (period.From - period.To).TotalMinutes / candleSticks.Count;
            for(int i = 0; i<candleSticks.Count; i++)
            {
                var offsetIndex = (candleSticks.Count / 2 + 1 + i) - candleSticks.Count;
                var price = (decimal)(avgPrice - (offsetIndex * (avgPrice * 0.1)));
                var candleStick = candleSticks[i];
                candleStick.HighPrice = candleStickValueType == Variable.CandleStickValueType.HighPrice ? price : 0;
                candleStick.LowPrice = candleStickValueType == Variable.CandleStickValueType.LowPrice ? price : 0;
                candleStick.OpenPrice = candleStickValueType == Variable.CandleStickValueType.OpenPrice ? price : 0;
                candleStick.ClosePrice = candleStickValueType == Variable.CandleStickValueType.ClosePrice ? price : 0;
                candleStick.PeriodDateTime = period.From.AddMinutes(i * periodMinutes);
            }

            return candleSticks;
        }
    }
}
