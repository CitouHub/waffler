using System;

namespace Waffler.Domain
{
    public class CandleStickSyncStatusDTO
    {
        public DateTime? FirstPeriodDateTime { get; set; }
        public DateTime? LastPeriodDateTime { get; set; }

        public bool Finished
        {
            get
            {
                if (LastPeriodDateTime == null)
                {
                    return false;
                }

                return LastPeriodDateTime.Value >= DateTime.UtcNow.AddHours(-1);
            }
        }

        public decimal Progress
        {
            get
            {
                if(FirstPeriodDateTime == null || LastPeriodDateTime == null)
                {
                    return 0;
                }

                var totalMinutes = (DateTime.UtcNow - FirstPeriodDateTime.Value).TotalMinutes;
                var minutesProsessed = (DateTime.UtcNow - LastPeriodDateTime.Value).TotalMinutes;
                var progress = Math.Round((100 - ((decimal)minutesProsessed / (decimal)totalMinutes) * 100), 2);

                return progress > 100 ? 100 : progress;
            }
        }
    }
}
