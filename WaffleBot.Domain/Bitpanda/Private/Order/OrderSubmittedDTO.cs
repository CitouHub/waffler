using System;

namespace WaffleBot.Domain.Bitpanda.Private.Order
{
    public class OrderSubmittedDTO
    {
        public string Order_id { get; set; }
        public string Account_id { get; set; }
        public string Instrument_code { get; set; }
        public decimal Amount { get; set; }
        public decimal Filled_amount { get; set; }
        public string Side { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public DateTime Time { get; set; }
        public string Time_in_force { get; set; }
        public bool? Is_post_only { get; set; }
        public decimal? Trigger_price { get; set; }
        public string Client_id { get; set; }
    }
}
