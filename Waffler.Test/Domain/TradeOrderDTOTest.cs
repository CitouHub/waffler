using Waffler.Domain;
using Xunit;

namespace Waffler.Test.Domain
{
    public class TradeOrderDTOTest
    {
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(100, 0, 0)]
        [InlineData(0, 100, 0)]
        [InlineData(100, 0.5, 50)]
        [InlineData(100, 1, 100)]
        [InlineData(100, 0.333333, 33.33)]
        [InlineData(100, 0.666666, 66.67)]
        public void TotalValuee(decimal price, decimal filledAmount, decimal expectedTotalValue)
        {
            //Act
            var trend = new TradeOrderDTO()
            {
                Price = price,
                FilledAmount = filledAmount
            };

            //Assert
            Assert.Equal(expectedTotalValue, trend.TotalValue);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(100, 0, 0)]
        [InlineData(0, 100, 0)]
        [InlineData(100, 50, 50)]
        [InlineData(100, 1, 1)]
        [InlineData(100, 33.3333, 33.33)]
        [InlineData(100, 66.6666, 66.67)]
        public void FilledPercent(decimal amount, decimal filledAmount, decimal expectedFilledPercent)
        {
            //Act
            var trend = new TradeOrderDTO()
            {
                Amount = amount,
                FilledAmount = filledAmount
            };

            //Assert
            Assert.Equal(expectedFilledPercent, trend.FilledPercent);
        }
    }
}
