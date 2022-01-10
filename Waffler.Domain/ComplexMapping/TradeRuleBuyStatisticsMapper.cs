using Waffler.Data.ComplexModel;

namespace Waffler.Domain.ComplexMapping
{
    public static class TradeRuleBuyStatisticsMapper
    {
        public static string GetTradeRuleName(sp_getTradeRuleBuyStatistics_Result tradeRuleBuyStatistics)
        {
            if (tradeRuleBuyStatistics.TradeRuleId == null || tradeRuleBuyStatistics.TradeRuleId == 0)
            {
                return "Manual";
            }

            return tradeRuleBuyStatistics.TradeRuleName + (tradeRuleBuyStatistics.TradeRuleIsDeleted != null && tradeRuleBuyStatistics.TradeRuleIsDeleted.Value ? " (Deleted)" : "");
        }
    }
}
