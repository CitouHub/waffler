using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class TradeRuleCondition
    {
        public int Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
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

        public virtual CandleStickValueType FromCandleStickValueType { get; set; }
        public virtual TradeRuleConditionPeriodDirection FromTradeRuleConditionPeriodDirection { get; set; }
        public virtual CandleStickValueType ToCandleStickValueType { get; set; }
        public virtual TradeRuleConditionPeriodDirection ToTradeRuleConditionPeriodDirection { get; set; }
        public virtual TradeRule TradeRule { get; set; }
        public virtual TradeRuleConditionComparator TradeRuleConditionComparator { get; set; }
    }
}
