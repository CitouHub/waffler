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
        public short TradeRuleConditionSampleDirectionId { get; set; }
        public string TradeRuleConditionSampleDirectionName { get; set; }
        public short CandleStickValueTypeId { get; set; }
        public string CandleStickValueTypeName { get; set; }
        public int FromMinutesOffset { get; set; }
        public int ToMinutesOffset { get; set; }
        public int FromMinutesSample { get; set; }
        public int ToMinutesSample { get; set; }
        public decimal DeltaPercent { get; set; }
        public string Description { get; set; }
        public bool IsOn { get; set; }

        //Consolidation of span and sampling for ease of use
        public Variable.TimeUnit SpanTimeUnit { get { return TimeUnitManager.GetTimeUnit(FromMinutesOffset); } }
        public int FromTime { get { return -1 * TimeUnitManager.GetTimeValue(SpanTimeUnit, FromMinutesOffset); } }
        public int ToTime { get { return -1 * TimeUnitManager.GetTimeValue(SpanTimeUnit, ToMinutesOffset); } }
        
        public Variable.TimeUnit SampleTimeUnit { get { return TimeUnitManager.GetTimeUnit(FromMinutesSample); } }
        public int FromSample { get { return TimeUnitManager.GetTimeValue(SampleTimeUnit, FromMinutesSample); } }
        public int ToSample { get { return TimeUnitManager.GetTimeValue(SampleTimeUnit, ToMinutesSample); } }
    }
}
