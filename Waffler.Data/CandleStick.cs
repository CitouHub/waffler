using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class CandleStick
    {
        public long Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public short TradeTypeId { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Volume { get; set; }
        public DateTime PeriodDateTime { get; set; }

        public virtual TradeType TradeType { get; set; }
    }
}
