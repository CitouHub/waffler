﻿namespace Waffler.Domain
{
    public class TradeRuleConditionDTO
    {
        public int Id { get; set; }
        public int TradeRuleId { get; set; }
        public short CandleStickValueTypeId { get; set; }
        public short TradeRuleConditionComparatorId { get; set; }
        public short TradeRuleConditionSampleDirectionId { get; set; }
        public int FromMinutesOffset { get; set; }
        public int ToMinutesOffset { get; set; }
        public int FromMinutesSample { get; set; }
        public int ToMinutesSample { get; set; }
        public decimal DeltaPercent { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
    }
}