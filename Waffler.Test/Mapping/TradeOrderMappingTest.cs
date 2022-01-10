using AutoMapper;
using Xunit;

using Waffler.Domain;
using Waffler.Test.Helper;

namespace Waffler.Test.Mapping
{
    public class TradeOrderMappingTest
    {
        private readonly IMapper _mapper;

        public TradeOrderMappingTest()
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
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeRuleId = 0;

            //Act
            var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(tradeOrder);

            //Assert
            Assert.Equal(0, tradeOrderDTO.TradeRuleId);
        }

        [Fact]
        public void TradeRuleId_Null_0()
        {
            //Setup
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeRuleId = null;

            //Act
            var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(tradeOrder);

            //Assert
            Assert.Equal(0, tradeOrderDTO.TradeRuleId);
        }

        [Fact]
        public void TradeRuleName_0_Manual()
        {
            //Setup
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeRuleId = 0;

            //Act
            var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(tradeOrder);

            //Assert
            Assert.Equal("Manual", tradeOrderDTO.TradeRuleName);
        }

        [Fact]
        public void TradeRuleName_Null_Manual()
        {
            //Setup
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeRuleId = null;

            //Act
            var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(tradeOrder);

            //Assert
            Assert.Equal("Manual", tradeOrderDTO.TradeRuleName);
        }

        [Fact]
        public void TradeRuleName_Deleted()
        {
            //Setup
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeRule = TradeRuleHelper.GetTradeRule();
            tradeOrder.TradeRule.Name = "Test";
            tradeOrder.TradeRule.IsDeleted = true;

            //Act
            var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(tradeOrder);

            //Assert
            Assert.Equal($"{tradeOrder.TradeRule.Name} (Deleted)", tradeOrderDTO.TradeRuleName);
        }
    }
}
