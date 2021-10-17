namespace Waffler.Domain
{
    public class PriceTrendsDTO
    {
		public decimal HighPriceTrend { get; set; }
		public decimal LowPriceTrend { get; set; }
		public decimal OpenPriceTrend { get; set; }
		public decimal ClosePriceTrend { get; set; }
        public decimal HighLowPriceTrend { get; set; }
        public decimal OpenClosePriceTrend { get; set; }
        public decimal AvgHighLowPriceTrend { get; set; }
		public decimal AvgOpenClosePriceTrend { get; set; }

        public override string ToString()
        {
            return $"H: {HighPriceTrend}, " +
                $"L: {LowPriceTrend}, " +
                $"O: {OpenPriceTrend}, " +
                $"C: {ClosePriceTrend}, " +
                $"HL: {HighLowPriceTrend}, " +
                $"OC: {OpenClosePriceTrend}, " +
                $"AHL: {AvgHighLowPriceTrend}, " +
                $"AOC: {AvgOpenClosePriceTrend}";
        }
    }
}
