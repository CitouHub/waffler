using System;

using Waffler.Common;
using Waffler.Domain;

namespace Waffler.Test.Helper
{
    public static class TradeRuleHelper
    {
        public static TradeRuleDTO GetTradeRuleDTO()
        {
            return new TradeRuleDTO()
            {
                TradeActionId = (short)Variable.TradeAction.Buy,
                TradeTypeId = (short)Variable.TradeType.BTC_EUR,
                TradeConditionOperatorId = (short)Variable.TradeConditionOperator.AND,
                CandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice,
                TradeRuleStatusId = (short)Variable.TradeRuleStatus.Active,
                LastTrigger = DateTime.MinValue,
                Name = "Test trade rule",
                Amount = 0,
                TradeMinIntervalMinutes = (int)TimeSpan.FromDays(1).TotalMinutes
            };
        }
    }
}
