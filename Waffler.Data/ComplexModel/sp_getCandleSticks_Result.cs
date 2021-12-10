using System;

namespace Waffler.Data.ComplexModel
{
    public class sp_getCandleSticks_Result
    {
		public decimal HighPrice { get; set; }
		public decimal LowPrice { get; set; }
		public decimal OpenPrice { get; set; }
		public decimal ClosePrice { get; set; }
		public decimal Volume { get; set; }
		public DateTime PeriodDateTime { get; set; }
	}
}
