using System;

using Waffler.Domain;
using Waffler.Domain.Message;

namespace Waffler.Test.Helper
{
    public static class TradeRuleTestQueueHelper
    {
        public static TradeRuleTestStatusDTO GetTradeRuleTestStatusDTO()
        {
            var date = DateTime.UtcNow;
            return new TradeRuleTestStatusDTO()
            {
                Aborted = false,
                CurrentPositionDate = date,
                FromDate = date,
                ToDate = date,
                TradeRuleId = 1
            };
        }

        public static TradeRuleTestRequestDTO GetTradeRuleTestRequestDTO()
        {
            var date = DateTime.UtcNow;
            return new TradeRuleTestRequestDTO()
            {
                FromDate = date,
                MinuteStep = 15,
                ToDate = date,
                TradeRuleId = 1
            };
        }
    }
}
