using System;
using System.Collections.Generic;
using System.Linq;

using Waffler.Domain;
using static Waffler.Common.Variable;

namespace Waffler.Function.Util
{
    public static class TradeRuleHelper
    {
        public static bool EvaluateRule(List<TradeRuleConditionEvaluationDTO> conditionResult, TradeConditionOperator tradeConditionOperator)
        {
            if (conditionResult.Any() == false)
            {
                return false;
            }

            switch (tradeConditionOperator)
            {
                case TradeConditionOperator.AND:
                    return conditionResult.All(_ => _.IsFullfilled == true);
                case TradeConditionOperator.OR:
                    return conditionResult.Any(_ => _.IsFullfilled == true);
                default:
                    break;
            }

            return false;
        }

        public static bool EvaluateCondition(TradeRuleConditionDTO condition, decimal value)
        {
            switch ((TradeRuleConditionComparator)condition.TradeRuleConditionComparatorId)
            {
                case TradeRuleConditionComparator.MoreThen:
                    return value > condition.DeltaPercent;
                case TradeRuleConditionComparator.LessThen:
                    return value < condition.DeltaPercent;
                case TradeRuleConditionComparator.AbsMoreThen:
                    return Math.Abs(value) > condition.DeltaPercent;
                case TradeRuleConditionComparator.AbsLessThen:
                    return Math.Abs(value) < condition.DeltaPercent;
                default:
                    break;
            }

            return false;
        }

        public static decimal GetTargetValue(TradeRuleConditionDTO condition, PriceTrendsDTO trends)
        {
            switch ((CandleStickValueType)condition.CandleStickValueTypeId)
            {
                case CandleStickValueType.HighPrice:
                    return trends.HighPriceTrend;
                case CandleStickValueType.LowPrice:
                    return trends.LowPriceTrend;
                case CandleStickValueType.OpenPrice:
                    return trends.OpenPriceTrend;
                case CandleStickValueType.ClosePrice:
                    return trends.ClosePriceTrend;
                case CandleStickValueType.HighLowPrice:
                    return trends.HighLowPriceTrend;
                case CandleStickValueType.OpenClosePrice:
                    return trends.OpenClosePriceTrend;
                case CandleStickValueType.AvgHighLowPrice:
                    return trends.AvgHighLowPriceTrend;
                case CandleStickValueType.AvgOpenClosePrice:
                    return trends.AvgHighLowPriceTrend;
                default:
                    break;
            }

            return default;
        }
    }
}
