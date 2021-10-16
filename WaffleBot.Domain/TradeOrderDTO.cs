using System;
using System.Collections.Generic;
using System.Text;

namespace WaffleBot.Domain
{
    public class TradeOrderDTO
    {
        public int Id { get; set; }
        public int TradeRuleId { get; set; }
        public short TradeOrderStatusId { get; set; }
        public Guid OrderId { get; set; }
        public string InstrumentCode { get; set; }
        public DateTime OrderDateTime { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal FilledAmount { get; set; }
    }
}
