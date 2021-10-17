namespace Waffler.Data.ComplexModel
{
    public class sp_getPriceTrends_Result
    {
		public decimal HighPriceTrend { get; set; }
		public decimal LowPriceTrend { get; set; }
		public decimal OpenPriceTrend { get; set; }
		public decimal ClosePriceTrend { get; set; }
		public decimal HighLowPriceTrend { get; set; }
		public decimal OpenClosePriceTrend { get; set; }
		public decimal AvgHighLowPriceTrend { get; set; }
		public decimal AvgOpenClosePriceTrend { get; set; }
	}
}
