using System;

namespace WaffleBot.Domain.Bitpanda.Private
{
    public class CreateOrderDTO
    {
        public string Instrument_code { get; set; }
        public string Type { get; set; }
        public string Side { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public string Client_id { get; set; }
        public string Time_in_force { get; set; }
        public DateTime? Expire_after { get; set; }
        public bool? Is_post_only { get; set; }
    }
}
