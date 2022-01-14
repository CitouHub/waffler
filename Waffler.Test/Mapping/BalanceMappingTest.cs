using AutoMapper;
using Xunit;
using Waffler.Domain;

namespace Waffler.Test.Mapping
{
    public class BalanceMappingTest
    {
        private readonly IMapper _mapper;

        public BalanceMappingTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Theory]
        [InlineData(Common.Bitpanda.CurrencyCode.BTC, 0.000000005, 0.00000000)]
        [InlineData(Common.Bitpanda.CurrencyCode.BTC, 0.000000006, 0.00000001)]
        [InlineData(Common.Bitpanda.CurrencyCode.BTC, 0.00000007, 0.00000007)]
        [InlineData(Common.Bitpanda.CurrencyCode.EUR, 0.005, 0.00)]
        [InlineData(Common.Bitpanda.CurrencyCode.EUR, 0.006, 0.01)]
        [InlineData(Common.Bitpanda.CurrencyCode.EUR, 0.07, 0.07)]
        public void Availible(string currencyCode, decimal value, decimal expectedValue)
        {
            //Setup
            var balanace = new Waffler.Domain.Bitpanda.Private.Balance.BalanceDTO()
            {
                Currency_code = currencyCode,
                Available = value
            };

            //Act
            var balanceDTO = _mapper.Map<BalanceDTO>(balanace);

            //Assert
            Assert.Equal(balanceDTO.Available, expectedValue);
        }

        [Theory]
        [InlineData(Common.Bitpanda.CurrencyCode.BTC, 0.000000005, 0.00000000)]
        [InlineData(Common.Bitpanda.CurrencyCode.BTC, 0.000000006, 0.00000001)]
        [InlineData(Common.Bitpanda.CurrencyCode.BTC, 0.00000007, 0.00000007)]
        [InlineData(Common.Bitpanda.CurrencyCode.EUR, 0.005, 0.00)]
        [InlineData(Common.Bitpanda.CurrencyCode.EUR, 0.006, 0.01)]
        [InlineData(Common.Bitpanda.CurrencyCode.EUR, 0.07, 0.07)]
        public void Locked(string currencyCode, decimal value, decimal expectedValue)
        {
            //Setup
            var balanace = new Waffler.Domain.Bitpanda.Private.Balance.BalanceDTO()
            {
                Currency_code = currencyCode,
                Locked = value
            };

            //Act
            var balanceDTO = _mapper.Map<BalanceDTO>(balanace);

            //Assert
            Assert.Equal(balanceDTO.TradeLocked, expectedValue);
        }
    }
}
