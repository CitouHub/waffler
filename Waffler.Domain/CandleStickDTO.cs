using System;
using System.Text.Json.Serialization;

namespace Waffler.Domain
{
    public class CandleStickDTO
    {
        [JsonIgnore]
        public short? TradeTypeId { get; set; }
        [JsonPropertyName("high")]
        public decimal HighPrice { get; set; }
        [JsonPropertyName("low")]
        public decimal LowPrice { get; set; }
        [JsonPropertyName("open")]
        public decimal OpenPrice { get; set; }
        [JsonPropertyName("close")]
        public decimal ClosePrice { get; set; }
        [JsonIgnore]
        public decimal? AvgHighLowPrice { get; set; }
        [JsonIgnore]
        public decimal? AvgOpenClosePrice { get; set; }
        [JsonIgnore]
        public decimal? TotalAmount { get; set; }
        [JsonPropertyName("volume")]
        public decimal Volume { get; set; }
        [JsonPropertyName("date")]
        public DateTime PeriodDateTime { get; set; }
    }
}
