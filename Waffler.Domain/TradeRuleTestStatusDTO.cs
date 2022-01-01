using System;

namespace Waffler.Domain
{
    public class TradeRuleTestStatusDTO
    {
        public int TradeRuleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CurrentPositionDate { get; set; }
        public bool Aborted { get; set; }
        public decimal Progress { get
            {
                var totalMinutes = (ToDate - FromDate).TotalMinutes;
                if(totalMinutes <= 0)
                {
                    return 100;
                }

                var minutesProsessed = (ToDate - CurrentPositionDate).TotalMinutes;
                var progress = Math.Round((100 - ((decimal)minutesProsessed / (decimal)totalMinutes) * 100), 2);

                return progress > 100 ? 100 : progress;
            }
        }
    }
}