#pragma warning disable IDE1006 // Naming Styles
namespace Waffler.Data.ComplexModel
{
    public class sp_getTradeRuleBuyStatistics_Result
	{
		public int TradeRuleId { get; set; }
		public string TradeRuleName { get; set; }
		public int Orders { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal TotalFilledAmount { get; set; }
		public decimal FilledPercent { get; set; }
		public decimal TotalInvested { get; set; }
		public decimal AveragePrice { get; set; }
		public decimal? ValueIncrease { get; set; }
	}
}