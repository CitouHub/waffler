namespace Waffler.Domain.Export
{
    public class TradeRuleConditionExportDTO
    {
        public int TradeRuleId { get; set; }
        public short TradeRuleConditionComparatorId { get; set; }
        public short FromCandleStickValueTypeId { get; set; }
        public short FromTradeRuleConditionPeriodDirectionId { get; set; }
        public int FromMinutes { get; set; }
        public int FromPeriodMinutes { get; set; }
        public short ToCandleStickValueTypeId { get; set; }
        public short ToTradeRuleConditionPeriodDirectionId { get; set; }
        public int ToMinutes { get; set; }
        public int ToPeriodMinutes { get; set; }
        public decimal DeltaPercent { get; set; }
        public string Description { get; set; }
        public bool IsOn { get; set; }
    }
}
