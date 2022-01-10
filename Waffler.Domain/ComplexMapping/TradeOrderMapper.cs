using Waffler.Data;

namespace Waffler.Domain.ComplexMapping
{
    public static class TradeOrderMapper
    {
        public static string GetTradeRuleName(TradeOrder tradeOrder)
        {
            if(tradeOrder.TradeRuleId == null || tradeOrder.TradeRuleId == 0)
            {
                return "Manual";
            }

            if(tradeOrder.TradeRule != null)
            {
                return tradeOrder.TradeRule.Name + (tradeOrder.TradeRule.IsDeleted ? " (Deleted)" : "");
            }

            return null;
        }
    }
}