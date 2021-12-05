using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class TradeOrder
    {
        public int Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public short? TradeActionId { get; set; }
        public short TradeOrderStatusId { get; set; }
        public int? TradeRuleId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal FilledAmount { get; set; }
        public bool? IsActive { get; set; }

        public virtual TradeAction TradeAction { get; set; }
        public virtual TradeOrderStatus TradeOrderStatus { get; set; }
        public virtual TradeRule TradeRule { get; set; }
    }
}
