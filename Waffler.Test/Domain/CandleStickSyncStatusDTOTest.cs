using System;
using Waffler.Domain;
using Xunit;

namespace Waffler.Test.Domain
{
    public class CandleStickSyncStatusDTOTest
    {
        [Theory]
        [InlineData(59, true)]
        [InlineData(61, false)]
        public void CandleStickSyncStatus_Finished(int lastPeriodDateTimeMinutesOffset, bool expectedFinished)
        {
            //Act
            var candleStickSyncStatus = new CandleStickSyncStatusDTO()
            {
                LastPeriodDateTime = DateTime.UtcNow.AddMinutes(-1 * lastPeriodDateTimeMinutesOffset)
            };

            //Assert
            Assert.Equal(expectedFinished, candleStickSyncStatus.Finished);
        }

        [Theory]
        [InlineData(null, null, 0)]
        [InlineData(400, null, 0)]
        [InlineData(null, 400, 0)]
        [InlineData(400, 400, 0)]
        [InlineData(400, 200, 50)]
        [InlineData(400, 0, 100)]
        public void CandleStickSyncStatus_Progress(int? firstPeriodDateTimeMinutesOffset, int? lastPeriodDateTimeMinutesOffset, decimal expectedProgress)
        {
            //Act
            var candleStickSyncStatus = new CandleStickSyncStatusDTO()
            {
                FirstPeriodDateTime = firstPeriodDateTimeMinutesOffset != null ? DateTime.UtcNow.AddMinutes(-1 * firstPeriodDateTimeMinutesOffset.Value) : null,
                LastPeriodDateTime = lastPeriodDateTimeMinutesOffset != null ? DateTime.UtcNow.AddMinutes(-1 * lastPeriodDateTimeMinutesOffset.Value) : null
            };

            //Assert
            Assert.Equal(expectedProgress, candleStickSyncStatus.Progress);
        }
    }
}
