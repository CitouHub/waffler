using Newtonsoft.Json;
using System;

namespace Waffler.Domain.Bitpanda.Private.Order
{
    public class OrderDTO
    {
        public string Order_id { get; set; }
        public string Account_id { get; set; }
        public string Client_id { get; set; }
        public string Instrument_code { get; set; }
        public decimal Amount { get; set; }
        public decimal Filled_amount { get; set; }
        public string Side { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public long Sequence { get; set; }
        public long Order_book_sequence { get; set; }
        public long? Update_modification_sequence { get; set; }
        public decimal Price { get; set; }
        public decimal Average_price { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public string Time_in_force { get; set; }
        public DateTime Time_last_updated { get; set; }
        public DateTime? Expire_after { get; set; }
        public bool? Is_post_only { get; set; }
        public DateTime? Time_triggered { get; set; }
        public decimal? Rigger_price { get; set; }
    }
}
