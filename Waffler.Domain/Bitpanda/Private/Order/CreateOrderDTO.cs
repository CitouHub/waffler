using Newtonsoft.Json;
using System;

namespace Waffler.Domain.Bitpanda.Private.Order
{
    public class CreateOrderDTO
    {
        [JsonProperty("instrument_code")]
        public string Instrument_code { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("client_id")]
        public string Client_id { get; set; }

        [JsonProperty("time_in_force")]
        public string Time_in_force { get; set; }

        [JsonProperty("expire_after")]
        public DateTime? Expire_after { get; set; }

        [JsonProperty("is_post_only")]
        public bool? Is_post_only { get; set; }
    }
}
