using System.Collections.Generic;

namespace Waffler.Domain.Export
{
    public class TradeRuleExportDTO
    {
        public short TradeActionId { get; set; }
        public short TradeTypeId { get; set; }
        public short TradeConditionOperatorId { get; set; }
        public short TradeRuleStatusId { get; set; }
        public short CandleStickValueTypeId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal PriceDeltaPercent { get; set; }
        public int TradeMinIntervalMinutes { get; set; }
        public int? TradeOrderExpirationMinutes { get; set; }

        public List<TradeRuleConditionExportDTO> TradeRuleConditions { get; set; }
    }
}
