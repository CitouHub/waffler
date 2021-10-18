using Newtonsoft.Json;
using System;

namespace Waffler.Domain
{
    public class CandleStickDTO
    {
        [JsonIgnore]
        public short TradeTypeId { get; set; }
        [JsonProperty("high")]
        public decimal HighPrice { get; set; }
        [JsonProperty("low")]
        public decimal LowPrice { get; set; }
        [JsonProperty("open")]
        public decimal OpenPrice { get; set; }
        [JsonProperty("close")]
        public decimal ClosePrice { get; set; }
        [JsonIgnore]
        public decimal AvgHighLowPrice { get; set; }
        [JsonIgnore]
        public decimal AvgOpenClosePrice { get; set; }
        [JsonIgnore]
        public decimal TotalAmount { get; set; }
        [JsonProperty("volume")]
        public decimal Volume { get; set; }
        [JsonProperty("date")]
        public DateTime PeriodDateTime { get; set; }
    }
}
