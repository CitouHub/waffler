using System;

using Waffler.Domain;

namespace Waffler.Service.Util
{
    public static class DataSyncHandler
    {
        public static readonly TimeSpan ValidSyncOffser = TimeSpan.FromMinutes(15);

        public static bool IsDataSynced(CandleStickDTO lastCandleStick)
        {
            return lastCandleStick != null && Math.Abs((decimal)(DateTime.UtcNow - lastCandleStick.PeriodDateTime).TotalMinutes) < (int)ValidSyncOffser.TotalMinutes;
        }
    }
}
