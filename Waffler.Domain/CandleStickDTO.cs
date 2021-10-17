using System;

namespace Waffler.Domain
{
    public class CandleStickDTO
    {
        public short TradeTypeId { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal AvgHighLowPrice { get; set; }
        public decimal AvgOpenClosePrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Volume { get; set; }
        public DateTime PeriodDateTime { get; set; }
    }
}
