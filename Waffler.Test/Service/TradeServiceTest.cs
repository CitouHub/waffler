using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

using Waffler.Common;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Test.Helper;
using Waffler.Domain.Statistics;

#pragma warning disable IDE0028 // Simplify collection initialization
namespace Waffler.Test
{
    public class TradeServiceTest
    {
        private readonly ICandleStickService _candleStickService = Substitute.For<ICandleStickService>();
        private readonly ITradeRuleService _tradeRuleService = Substitute.For<ITradeRuleService>();
        private readonly ITradeOrderService _tradeOrderService = Substitute.For<ITradeOrderService>();
        private readonly IBitpandaService _bitpandaService = Substitute.For<IBitpandaService>();
        private readonly IStatisticsService _statisticsService = Substitute.For<IStatisticsService>();
        private readonly TradeService _tradeService;

        private readonly int TestTradeRuleId = 1;

        public TradeServiceTest()
        {
            var logger = Substitute.For<ILogger<TradeService>>();

            _tradeService = new TradeService(logger, _candleStickService, _tradeRuleService, _tradeOrderService, _bitpandaService, _statisticsService);
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_TradeRuleStatusInactive()
        {
            //Setup
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TradeRuleStatusId = (short)Variable.TradeRuleStatus.Inactive;
            tradeRule.LastTrigger = DateTime.UtcNow.AddDays(-1).AddMinutes(-1);
            tradeRule.TradeMinIntervalMinutes = 24 * 60;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, DateTime.UtcNow);

            //Assert
            Assert.Null(result);
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_TriggerInterval()
        {
            //Setup
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TradeRuleStatusId = (short)Variable.TradeRuleStatus.Active;
            tradeRule.LastTrigger = DateTime.UtcNow.AddDays(-1).AddMinutes(1);
            tradeRule.TradeMinIntervalMinutes = 24 * 60;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, DateTime.UtcNow);

            //Assert
            Assert.Null(result);
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_NoConditions()
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            Assert.Null(result);
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_ConditionsOff()
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            tradeRuleCondition.IsOn = false;
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == false));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.DidNotReceive().GetPriceTrendAsync(Arg.Any<DateTime>(), Arg.Any<Variable.TradeType>(), Arg.Any<TradeRuleConditionDTO>());
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            _ = _tradeRuleService.DidNotReceive().UpdateTradeRuleAsync(Arg.Any<TradeRuleDTO>());
        }

        [Fact]
        public async Task HandleTradeRule_NoTrade_NoTrend()
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == false));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            _ = _tradeRuleService.DidNotReceive().UpdateTradeRuleAsync(Arg.Any<TradeRuleDTO>());
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleConditionComparator.LessThen, 1.0, 1.1)]
        [InlineData((short)Variable.TradeRuleConditionComparator.LessThen, -1.0, -0.9)]
        [InlineData((short)Variable.TradeRuleConditionComparator.MoreThen, 1.0, 0.9)]
        [InlineData((short)Variable.TradeRuleConditionComparator.MoreThen, -1.0, -1.1)]
        [InlineData((short)Variable.TradeRuleConditionComparator.AbsLessThen, 1.0, -1.1)]
        [InlineData((short)Variable.TradeRuleConditionComparator.AbsLessThen, 1.0, 1.1)]
        [InlineData((short)Variable.TradeRuleConditionComparator.AbsMoreThen, 1.0, -0.9)]
        [InlineData((short)Variable.TradeRuleConditionComparator.AbsMoreThen, 1.0, 0.9)]
        public async Task HandleTradeRule_NoTrade_TradeRuleConditionNotFulfilled(short tradeRuleConditionComparatorId, decimal deltaPercent, decimal trend)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            tradeRuleCondition.TradeRuleConditionComparatorId = tradeRuleConditionComparatorId;
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            tradeRuleCondition.DeltaPercent = deltaPercent;
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition))
                .Returns(StatisticsHelper.GetTrendDTO(trend));

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == false));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            _ = _tradeRuleService.DidNotReceive().UpdateTradeRuleAsync(Arg.Any<TradeRuleDTO>());
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.LessThen, 1.0, 0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.LessThen, -1.0, -1.1)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.MoreThen, 1.0, 1.1)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.MoreThen, -1.0, -0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.AbsLessThen, 1.0, -0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.AbsLessThen, 1.0, 0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.AbsMoreThen, 1.0, -1.1)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeRuleConditionComparator.AbsMoreThen, 1.0, 1.1)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.LessThen, 1.0, 0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.LessThen, -1.0, -1.1)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.MoreThen, 1.0, 1.1)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.MoreThen, -1.0, -0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.AbsLessThen, 1.0, -0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.AbsLessThen, 1.0, 0.9)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.AbsMoreThen, 1.0, -1.1)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeRuleConditionComparator.AbsMoreThen, 1.0, 1.1)]
        public async Task HandleTradeRule_Trade_TradeRuleConditionFulfilled(short tradeRuleStatusId, short tradeRuleConditionComparatorId, decimal deltaPercent, decimal trend)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            tradeRuleCondition.TradeRuleConditionComparatorId = tradeRuleConditionComparatorId;
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Amount = (decimal)0.01;
            tradeRule.TradeRuleStatusId = tradeRuleStatusId;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            tradeRuleCondition.DeltaPercent = deltaPercent;
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition))
                .Returns(StatisticsHelper.GetTrendDTO(trend));
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == true));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));
            _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            if(tradeRule.TradeRuleStatusId == (short)Variable.TradeRuleStatus.Active)
            {
                _ = _bitpandaService.Received().TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>());
            } 
            else
            {
                _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            }
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is(tradeRule));
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeConditionOperator.AND, -0.1, -1.2)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeConditionOperator.OR, 0.1, -1.2)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeConditionOperator.AND, -0.1, -1.2)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeConditionOperator.OR, 0.1, -1.2)]
        public async Task HandleTradeRule_Trade_MultiConditions(short tradeRuleStatusId, short tradeConditionOperatorId, decimal trend1, decimal trend2)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition1 = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRuleCondition2 = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TradeRuleStatusId = tradeRuleStatusId;
            tradeRule.TradeConditionOperatorId = tradeConditionOperatorId;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition1);
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition2);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition1))
                .Returns(StatisticsHelper.GetTrendDTO(trend1));
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition2))
                .Returns(StatisticsHelper.GetTrendDTO(trend2));
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            if(tradeConditionOperatorId == (short)Variable.TradeConditionOperator.AND)
            {
                Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == true));
            } 
            else
            {
                Assert.Contains(result.TradeRuleCondtionEvaluations, _ => _.IsFullfilled == true);
                Assert.Contains(result.TradeRuleCondtionEvaluations, _ => _.IsFullfilled == false);
            }
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition1));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition2));
            _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            if (tradeRule.TradeRuleStatusId == (short)Variable.TradeRuleStatus.Active)
            {
                _ = _bitpandaService.Received().TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>());
            }
            else
            {
                _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            }
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is(tradeRule));
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeConditionOperator.AND, -0.1, 1.2)]
        [InlineData((short)Variable.TradeRuleStatus.Active, (short)Variable.TradeConditionOperator.OR, 0.1, 1.2)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeConditionOperator.AND, -0.1, 1.2)]
        [InlineData((short)Variable.TradeRuleStatus.Test, (short)Variable.TradeConditionOperator.OR, 0.1, 1.2)]
        public async Task HandleTradeRule_NoTrade_MultiConditions(short tradeRuleStatusId, short tradeConditionOperatorId, decimal trend1, decimal trend2)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition1 = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRuleCondition2 = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TradeRuleStatusId = tradeRuleStatusId;
            tradeRule.TradeConditionOperatorId = tradeConditionOperatorId;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition1);
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition2);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition1))
                .Returns(StatisticsHelper.GetTrendDTO(trend1));
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition2))
                .Returns(StatisticsHelper.GetTrendDTO(trend2));
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            if (tradeConditionOperatorId == (short)Variable.TradeConditionOperator.AND)
            {
                Assert.Contains(result.TradeRuleCondtionEvaluations, _ => _.IsFullfilled == true);
                Assert.Contains(result.TradeRuleCondtionEvaluations, _ => _.IsFullfilled == false);
            }
            else
            {
                Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == false));
            }
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition1));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition2));
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            _ = _tradeRuleService.DidNotReceive().UpdateTradeRuleAsync(Arg.Any<TradeRuleDTO>());
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleStatus.Active, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Active, -2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, -2.5)]
        public async Task HandleTradeRule_TradeClosePrice_PriceAmount(short tradeRuleStatusId, decimal deltaPricePercent)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Amount = 100;
            tradeRule.PriceDeltaPercent = deltaPricePercent;
            tradeRule.TradeRuleStatusId = tradeRuleStatusId;
            tradeRule.CandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.HighPrice = 1500;
            candleStick.LowPrice = 500;
            candleStick.OpenPrice = 1200;
            candleStick.ClosePrice = 800;
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition))
                .Returns(StatisticsHelper.GetTrendDTO(-1));
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            var price = candleStick.HighPrice + (candleStick.HighPrice * (deltaPricePercent / 100));
            var amount = Math.Round(tradeRule.Amount / price, Bitpanda.DecimalPrecision);
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == true));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));

            if (tradeRule.TradeRuleStatusId == (short)Variable.TradeRuleStatus.Active)
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ =>
                    _.Price == price && _.Amount == amount && _.FilledAmount == 0 && _.IsActive == true && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Open));
                _ = _bitpandaService.Received().TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Is(amount), Arg.Is(price));
            }
            else
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ =>
                    _.Price == price && _.Amount == amount && _.FilledAmount == amount && _.IsActive == false && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test));
                _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            }
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is(tradeRule));
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleStatus.Active, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Active, -2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, -2.5)]
        public async Task HandleTradeRule_TradeLowProce_PriceAmount(short tradeRuleStatusId, decimal deltaPricePercent)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Amount = 100;
            tradeRule.PriceDeltaPercent = deltaPricePercent;
            tradeRule.TradeRuleStatusId = tradeRuleStatusId;
            tradeRule.CandleStickValueTypeId = (short)Variable.CandleStickValueType.LowPrice;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.HighPrice = 1500;
            candleStick.LowPrice = 500;
            candleStick.OpenPrice = 1200;
            candleStick.ClosePrice = 800;
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition))
                .Returns(StatisticsHelper.GetTrendDTO(-1));
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            var price = candleStick.LowPrice + (candleStick.LowPrice * (deltaPricePercent / 100));
            var amount = Math.Round(tradeRule.Amount / price, Bitpanda.DecimalPrecision);
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == true));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));

            if (tradeRule.TradeRuleStatusId == (short)Variable.TradeRuleStatus.Active)
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ =>
                    _.Price == price && _.Amount == amount && _.FilledAmount == 0 && _.IsActive == true && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Open));
                _ = _bitpandaService.Received().TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Is(amount), Arg.Is(price));
            }
            else
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ =>
                    _.Price == price && _.Amount == amount && _.FilledAmount == amount && _.IsActive == false && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test));
                _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            }
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is(tradeRule));
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleStatus.Active, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Active, -2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, -2.5)]
        public async Task HandleTradeRule_TradeOpenProce_PriceAmount(short tradeRuleStatusId, decimal deltaPricePercent)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Amount = 100;
            tradeRule.PriceDeltaPercent = deltaPricePercent;
            tradeRule.TradeRuleStatusId = tradeRuleStatusId;
            tradeRule.CandleStickValueTypeId = (short)Variable.CandleStickValueType.OpenPrice;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.HighPrice = 1500;
            candleStick.LowPrice = 500;
            candleStick.OpenPrice = 1200;
            candleStick.ClosePrice = 800;
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition))
                .Returns(StatisticsHelper.GetTrendDTO(-1));
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            var price = candleStick.OpenPrice + (candleStick.OpenPrice * (deltaPricePercent / 100));
            var amount = Math.Round(tradeRule.Amount / price, Bitpanda.DecimalPrecision);
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == true));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));

            if (tradeRule.TradeRuleStatusId == (short)Variable.TradeRuleStatus.Active)
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ =>
                    _.Price == price && _.Amount == amount && _.FilledAmount == 0 && _.IsActive == true && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Open));
                _ = _bitpandaService.Received().TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Is(amount), Arg.Is(price));
            }
            else
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ =>
                    _.Price == price && _.Amount == amount && _.FilledAmount == amount && _.IsActive == false && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test));
                _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            }
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is(tradeRule));
        }

        [Theory]
        [InlineData((short)Variable.TradeRuleStatus.Active, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Active, -2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, 2.5)]
        [InlineData((short)Variable.TradeRuleStatus.Test, -2.5)]
        public async Task HandleTradeRule_TradeCloseProce_PriceAmount(short tradeRuleStatusId, decimal deltaPricePercent)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Amount = 100;
            tradeRule.PriceDeltaPercent = deltaPricePercent;
            tradeRule.TradeRuleStatusId = tradeRuleStatusId;
            tradeRule.CandleStickValueTypeId = (short)Variable.CandleStickValueType.ClosePrice;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.HighPrice = 1500;
            candleStick.LowPrice = 500;
            candleStick.OpenPrice = 1200;
            candleStick.ClosePrice = 800;
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition))
                .Returns(StatisticsHelper.GetTrendDTO(-1));
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            var price = candleStick.ClosePrice + (candleStick.ClosePrice * (deltaPricePercent / 100));
            var amount = Math.Round(tradeRule.Amount / price, Bitpanda.DecimalPrecision);
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == true));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));

            if (tradeRule.TradeRuleStatusId == (short)Variable.TradeRuleStatus.Active)
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ => 
                    _.Price == price && _.Amount == amount && _.FilledAmount == 0 && _.IsActive == true && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Open));
                _ = _bitpandaService.Received().TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Is(amount), Arg.Is(price));
            }
            else
            {
                _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ => 
                    _.Price == price && _.Amount == amount && _.FilledAmount == amount && _.IsActive == false && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test));
                _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());
            }
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is(tradeRule));
        }

        [Theory]
        [InlineData((short)Variable.CandleStickValueType.HighPrice, 1000, 900, -11.0, false)]
        [InlineData((short)Variable.CandleStickValueType.HighPrice, 1000, 900, -9.0, true)]
        [InlineData((short)Variable.CandleStickValueType.LowPrice, 1000, 900, -11.0, false)]
        [InlineData((short)Variable.CandleStickValueType.LowPrice, 1000, 900, -9.0, true)]
        [InlineData((short)Variable.CandleStickValueType.OpenPrice, 1000, 900, -11.0, false)]
        [InlineData((short)Variable.CandleStickValueType.OpenPrice, 1000, 900, -9.0, true)]
        [InlineData((short)Variable.CandleStickValueType.ClosePrice, 1000, 900, -11.0, false)]
        [InlineData((short)Variable.CandleStickValueType.ClosePrice, 1000, 900, -9.0, true)]
        public async Task HandleTradeRule_TestTrade_FilledAmount(short candleStickValueTypeId, int orderPrice, int futurePrice, decimal deltaPricePercent, bool fullyFilled)
        {
            //Setup
            var currentPeriodDateTime = DateTime.UtcNow;
            var tradeRuleCondition = TradeRuleConditionHelper.GetTradeRuleConditionDTO();
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Amount = 100;
            tradeRule.TradeRuleStatusId = (short)Variable.TradeRuleStatus.Test;
            tradeRule.PriceDeltaPercent = deltaPricePercent;
            tradeRule.CandleStickValueTypeId = candleStickValueTypeId;
            tradeRule.TradeRuleConditions = new List<TradeRuleConditionDTO>();
            tradeRule.TradeRuleConditions.Add(tradeRuleCondition);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.HighPrice = candleStickValueTypeId == (short)Variable.CandleStickValueType.HighPrice ? orderPrice : 0;
            candleStick.LowPrice = candleStickValueTypeId == (short)Variable.CandleStickValueType.LowPrice ? orderPrice : 0;
            candleStick.OpenPrice = candleStickValueTypeId == (short)Variable.CandleStickValueType.OpenPrice ? orderPrice : 0;
            candleStick.ClosePrice = candleStickValueTypeId == (short)Variable.CandleStickValueType.ClosePrice ? orderPrice : 0;
            candleStick.PeriodDateTime = currentPeriodDateTime;
            var futureCandleStick = CandleStickHelper.GetCandleStickDTO();
            futureCandleStick.LowPrice = futurePrice;
            var orderSubmitted = BitpandaHelper.GetOrderSubmitted();

            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _candleStickService.GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime)).Returns(candleStick);
            _statisticsService.GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition))
                .Returns(StatisticsHelper.GetTrendDTO(-1));
            _candleStickService.GetCandleSticksAsync(Arg.Is(currentPeriodDateTime), Arg.Any<DateTime>(), Arg.Is(Variable.TradeType.BTC_EUR), Arg.Any<int>())
                .Returns(new List<CandleStickDTO>() { futureCandleStick });
            _bitpandaService.TryPlaceOrderAsync(Arg.Is(tradeRule), Arg.Any<decimal>(), Arg.Any<decimal>()).Returns(orderSubmitted);

            //Act
            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentPeriodDateTime);

            //Assert
            var price = orderPrice + (orderPrice * (tradeRule.PriceDeltaPercent / 100));
            var amount = Math.Round(tradeRule.Amount / price, Bitpanda.DecimalPrecision);
            var filledAmount = fullyFilled ? amount : 0;
            Assert.True(result.TradeRuleCondtionEvaluations.All(_ => _.IsFullfilled == true));
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Is(currentPeriodDateTime));
            _ = _statisticsService.Received().GetPriceTrendAsync(Arg.Is(currentPeriodDateTime), Arg.Any<Variable.TradeType>(), Arg.Is(tradeRuleCondition));

            _ = _tradeOrderService.Received().AddTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ =>
                _.Price == price && _.Amount == amount && _.FilledAmount == filledAmount && _.IsActive == false && _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test));
            _ = _bitpandaService.DidNotReceive().TryPlaceOrderAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<decimal>(), Arg.Any<decimal>());

            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is(tradeRule));
        }
    }
}
