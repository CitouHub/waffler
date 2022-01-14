using Xunit;
using Waffler.Domain.Statistics;

namespace Waffler.Test.Domain
{
    public class TradeRuleBuyStatisticsDTOTest
    {
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(100, 0, 0)]
        [InlineData(0, 100, 0)]
        [InlineData(50, 100, 50)]
        [InlineData(-50, 100, -50)]
        public void Return(decimal valueIncrease, decimal totalInvested, decimal expectedReturn)
        {
            //Setup
            var tradeRuleBuyStatistics = new TradeRuleBuyStatisticsDTO()
            {
                ValueIncrease = valueIncrease,
                TotalInvested = totalInvested
            };

            //Asset
            Assert.Equal(expectedReturn, tradeRuleBuyStatistics.Return);
        }
    }
}
