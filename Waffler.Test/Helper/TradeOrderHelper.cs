using System;

using Waffler.Common;
using Waffler.Data;
using Waffler.Domain;

namespace Waffler.Test.Helper
{
    public static class TradeOrderHelper
    {
        public static TradeOrderDTO GetTradeOrderDTO()
        {
            return new TradeOrderDTO()
            {
                Id = 1,
                Amount = 0,
                FilledAmount = 0,
                IsActive = true,
                OrderDateTime = DateTime.UtcNow,
                OrderId = Guid.NewGuid(),
                Price = 0,
                TradeActionId = (short)Variable.TradeAction.Buy,
                TradeOrderStatusId = (short)Variable.TradeOrderStatus.Test,
                TradeRuleId = 1
            };
        }

        public static TradeOrder GetTradeOrder()
        {
            return new TradeOrder()
            {
                Amount = 0,
                FilledAmount = 0,
                IsActive = true,
                OrderDateTime = DateTime.UtcNow,
                OrderId = Guid.NewGuid(),
                Price = 0,
                TradeActionId = (short)Variable.TradeAction.Buy,
                TradeOrderStatusId = (short)Variable.TradeOrderStatus.Test,
                TradeRuleId = 1
            };
        }
    }
}
