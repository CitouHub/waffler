namespace WaffleBot.Domain
{
    public class PriceTrendsDTO
    {
		public decimal HighPriceTrend { get; set; }
		public decimal LowPriceTrend { get; set; }
		public decimal OpenPriceTrend { get; set; }
		public decimal ClosePriceTrend { get; set; }
		public decimal AvgHighLowPriceTrend { get; set; }
		public decimal AvgOpenClosePriceTrend { get; set; }
	}
}
