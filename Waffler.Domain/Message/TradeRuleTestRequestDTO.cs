using System;

namespace Waffler.Domain.Message
{
    public class TradeRuleTestRequestDTO
    {
        public int TradeRuleId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int MinuteStep { get; set; }

        public override string ToString()
        {
            return $"{TradeRuleId} from {FromDate} to {ToDate} in {MinuteStep} minute steps";
        }
    }
}
