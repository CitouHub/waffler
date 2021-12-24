using Waffler.Common;
using Waffler.Domain;

namespace Waffler.Test.Helper
{
    public static class TradeRuleConditionHelper
    {
        public static TradeRuleConditionDTO GetTradeRuleCondition()
        {
            return new TradeRuleConditionDTO()
            {
                TradeRuleConditionComparatorId = (short)Variable.TradeRuleConditionComparator.LessThen,

                FromCandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice,
                FromTradeRuleConditionPeriodDirectionId = (short)Variable.TradeRuleConditionPeriodDirection.LeftShift,
                FromMinutes = -60,
                FromPeriodMinutes = 60,

                ToCandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice,
                ToTradeRuleConditionPeriodDirectionId = (short)Variable.TradeRuleConditionPeriodDirection.LeftShift,
                ToMinutes = 0,
                ToPeriodMinutes = 60,

                DeltaPercent = 0,
                Description = "Test trade rule condition",
                IsOn = true
            };
        }
    }
}
