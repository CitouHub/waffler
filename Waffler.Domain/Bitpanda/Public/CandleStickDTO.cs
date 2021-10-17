using System;

namespace Waffler.Domain.Bitpanda.Public
{
    public class CandleStickDTO
    {
        public long Last_Sequence { get; set; }
        public string Instrument_Code { get; set; }
        public GranularityDTO Granularity { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal Total_Amount { get; set; }
        public decimal Volume { get; set; }
        public DateTime Time { get; set; }
    }
}
