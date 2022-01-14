using System;

namespace Waffler.Domain.Statistics
{
    public class TradeRuleBuyStatisticsDTO
	{
		public int TradeRuleId { get; set; }
		public string TradeRuleName { get; set; }
		public int Orders { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal TotalFilledAmount { get; set; }
		public decimal FilledPercent { get; set; }
		public decimal TotalInvested { get; set; }
		public decimal AveragePrice { get; set; }
		public decimal ValueIncrease { get; set; }
		public decimal Return 
		{ 
			get
            {
				return Math.Round(ValueIncrease / 100 * TotalInvested, 2);

			}
		}
	}
}