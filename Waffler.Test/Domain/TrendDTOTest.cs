using Waffler.Domain.Statistics;
using Xunit;

namespace Waffler.Test.Domain
{
    public class TrendDTOTest
    {
        [Theory]
        [InlineData(0, 100, 0)]
        [InlineData(100, 0, -100)]
        [InlineData(100, 50, -50)]
        [InlineData(100, 150, 50)]
        [InlineData(100, 99, -1)]
        [InlineData(100, 101, 1)]
        public void Trend_Change(decimal fromPrice, decimal toPrice, decimal expectedChange)
        {
            //Act
            var trend = new TrendDTO()
            {
                FromPrice = fromPrice,
                ToPrice = toPrice
            };

            //Assert
            Assert.Equal(expectedChange, trend.Change);
        }
    }
}
