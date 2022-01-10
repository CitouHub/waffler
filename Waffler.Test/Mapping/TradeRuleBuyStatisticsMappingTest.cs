using AutoMapper;
using Xunit;

using Waffler.Domain;
using Waffler.Test.Helper;
using Waffler.Domain.Statistics;

namespace Waffler.Test.Mapping
{
    public class TradeRuleBuyStatisticsMappingTest
    {
        private readonly IMapper _mapper;

        public TradeRuleBuyStatisticsMappingTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void TradeRuleId_0_0()
        {
            //Setup
            var tradeRuleBuyStatistics = TradeRuleBuyStatisticsHelper.Get_sp_getTradeRuleBuyStatistics_Result();
            tradeRuleBuyStatistics.TradeRuleId = 0;

            //Act
            var tradeRuleBuyStatisticsDTO = _mapper.Map<TradeRuleBuyStatisticsDTO>(tradeRuleBuyStatistics);

            //Assert
            Assert.Equal(0, tradeRuleBuyStatisticsDTO.TradeRuleId);
        }

        [Fact]
        public void TradeRuleId_Null_0()
        {
            //Setup
            var tradeRuleBuyStatistics = TradeRuleBuyStatisticsHelper.Get_sp_getTradeRuleBuyStatistics_Result();
            tradeRuleBuyStatistics.TradeRuleId = null;

            //Act
            var tradeRuleBuyStatisticsDTO = _mapper.Map<TradeRuleBuyStatisticsDTO>(tradeRuleBuyStatistics);

            //Assert
            Assert.Equal(0, tradeRuleBuyStatisticsDTO.TradeRuleId);
        }

        [Fact]
        public void TradeRuleName_0_Manual()
        {
            //Setup
            var tradeRuleBuyStatistics = TradeRuleBuyStatisticsHelper.Get_sp_getTradeRuleBuyStatistics_Result();
            tradeRuleBuyStatistics.TradeRuleId = 0;

            //Act
            var tradeRuleBuyStatisticsDTO = _mapper.Map<TradeRuleBuyStatisticsDTO>(tradeRuleBuyStatistics);

            //Assert
            Assert.Equal("Manual", tradeRuleBuyStatisticsDTO.TradeRuleName);
        }

        [Fact]
        public void TradeRuleName_Null_Manual()
        {
            //Setup
            var tradeRuleBuyStatistics = TradeRuleBuyStatisticsHelper.Get_sp_getTradeRuleBuyStatistics_Result();
            tradeRuleBuyStatistics.TradeRuleId = null;

            //Act
            var tradeRuleBuyStatisticsDTO = _mapper.Map<TradeRuleBuyStatisticsDTO>(tradeRuleBuyStatistics);

            //Assert
            Assert.Equal("Manual", tradeRuleBuyStatisticsDTO.TradeRuleName);
        }

        [Fact]
        public void TradeRuleName_Deleted()
        {
            //Setup
            var tradeRuleBuyStatistics = TradeRuleBuyStatisticsHelper.Get_sp_getTradeRuleBuyStatistics_Result();
            tradeRuleBuyStatistics.TradeRuleId = 1;
            tradeRuleBuyStatistics.TradeRuleName = "Test";
            tradeRuleBuyStatistics.TradeRuleIsDeleted = true;

            //Act
            var tradeRuleBuyStatisticsDTO = _mapper.Map<TradeRuleBuyStatisticsDTO>(tradeRuleBuyStatistics);

            //Assert
            Assert.Equal($"{tradeRuleBuyStatistics.TradeRuleName} (Deleted)", tradeRuleBuyStatisticsDTO.TradeRuleName);
        }
    }
}
