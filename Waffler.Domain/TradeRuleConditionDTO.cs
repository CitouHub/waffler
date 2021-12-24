using Waffler.Common;
using Waffler.Common.Util;

namespace Waffler.Domain
{
    public class TradeRuleConditionDTO
    {
        public int Id { get; set; }
        public int TradeRuleId { get; set; }
        public short TradeRuleConditionComparatorId { get; set; }
        public string TradeRuleConditionComparatorName { get; set; }
        public short FromCandleStickValueTypeId { get; set; }
        public string FromCandleStickValueTypeName { get; set; }
        public short FromTradeRuleConditionPeriodDirectionId { get; set; }
        public string FromTradeRuleConditionPeriodDirectionName { get; set; }
        public int FromMinutes { get; set; }
        public int FromPeriodMinutes { get; set; }
        public short ToCandleStickValueTypeId { get; set; }
        public string ToCandleStickValueTypeName { get; set; }
        public short ToTradeRuleConditionPeriodDirectionId { get; set; }
        public string ToTradeRuleConditionPeriodDirectionName { get; set; }
        public int ToMinutes { get; set; }
        public int ToPeriodMinutes { get; set; }
        public decimal DeltaPercent { get; set; }
        public string Description { get; set; }
        public bool IsOn { get; set; }

        //Consolidation of span and sampling for ease of use
        public Variable.TimeUnit FromTimeUnit { get { return TimeUnitManager.GetTimeUnit(FromMinutes); } }
        public int FromTime { get { return -1 * TimeUnitManager.GetTimeValue(FromTimeUnit, FromMinutes); } }
        public Variable.TimeUnit FromPeriodTimeUnit { get { return TimeUnitManager.GetTimeUnit(FromPeriodMinutes); } }
        public int FromPeriod { get { return TimeUnitManager.GetTimeValue(FromPeriodTimeUnit, FromPeriodMinutes); } }

        public Variable.TimeUnit ToTimeUnit { get { return TimeUnitManager.GetTimeUnit(ToMinutes); } }
        public int ToTime { get { return -1 * TimeUnitManager.GetTimeValue(ToTimeUnit, ToMinutes); } }
        public Variable.TimeUnit ToPeriodTimeUnit { get { return TimeUnitManager.GetTimeUnit(ToPeriodMinutes); } }
        public int ToPeriod { get { return TimeUnitManager.GetTimeValue(ToPeriodTimeUnit, ToPeriodMinutes); } }
    }
}
