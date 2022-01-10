using AutoMapper;
using Xunit;

using Waffler.Domain;
using Waffler.Test.Helper;

namespace Waffler.Test.Mapping.Bitpanda
{
    public class OrderMappingTest
    {
        private readonly IMapper _mapper;

        public OrderMappingTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Theory]
        [InlineData(Common.Bitpanda.Status.CLOSED, false)]
        [InlineData(Common.Bitpanda.Status.FAILED, false)]
        [InlineData(Common.Bitpanda.Status.FILLED, true)]
        [InlineData(Common.Bitpanda.Status.FILLED_CLOSED, false)]
        [InlineData(Common.Bitpanda.Status.FILLED_FULLY, false)]
        [InlineData(Common.Bitpanda.Status.FILLED_REJECTED, false)]
        [InlineData(Common.Bitpanda.Status.OPEN, true)]
        [InlineData(Common.Bitpanda.Status.REJECTED, false)]
        [InlineData(Common.Bitpanda.Status.STOP_TRIGGERED, false)]
        public void IsActive(string orderStatus, bool expectedIsActive)
        {
            //Setup
            var order = BitpandaHelper.GetOrder();
            order.Status = orderStatus;

            //Act
            var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(order);

            //Assert
            Assert.Equal(expectedIsActive, tradeOrderDTO.IsActive);
        }
    }
}