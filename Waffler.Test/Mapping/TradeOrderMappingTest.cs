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
        public void TradeRule_Manual()
        {
            //Setup
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeRule = null;

            //Act
            var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(tradeOrder);

            //Assert
            Assert.Equal(0, tradeOrderDTO.TradeRuleId);
            Assert.Equal("Manual", tradeOrderDTO.TradeRuleName);
        }
    }
}
