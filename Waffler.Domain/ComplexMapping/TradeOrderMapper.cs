using Waffler.Data;

namespace Waffler.Domain.ComplexMapping
{
    public static class TradeOrderMapper
    {
        public static string GetTradeRuleName(TradeOrder tradeOrder)
        {
            if(tradeOrder.TradeRule == null)
            {
                return "Manual";
            }

            return tradeOrder.TradeRule.Name + (tradeOrder.TradeRule.IsDeleted ? " (Deleted)" : "");
        }
    }
}