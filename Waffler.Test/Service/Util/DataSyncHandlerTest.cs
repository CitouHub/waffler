using System;

using Xunit;

using Waffler.Service.Util;
using Waffler.Test.Helper;

namespace Waffler.Test.Service.Util
{
    public class DataSyncHandlerTest
    {
        [Fact]
        public void IsDataSynced_False()
        {
            //Setup
            var lastCandleStick = CandleStickHelper.GetCandleStickDTO();
            lastCandleStick.PeriodDateTime = DateTime.UtcNow.AddMinutes(-1 * DataSyncHandler.ValidSyncOffser.TotalMinutes);

            //Act
            var synced = DataSyncHandler.IsDataSynced(lastCandleStick);

            //Asert
            Assert.False(synced);
        }

        [Fact]
        public void IsDataSynced_True()
        {
            //Setup
            var lastCandleStick = CandleStickHelper.GetCandleStickDTO();
            lastCandleStick.PeriodDateTime = DateTime.UtcNow.AddMinutes(-1 * DataSyncHandler.ValidSyncOffser.TotalMinutes + 1);

            //Act
            var synced = DataSyncHandler.IsDataSynced(lastCandleStick);

            //Asert
            Assert.True(synced);
        }
    }
}
