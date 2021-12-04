using System;

namespace Waffler.Domain
{
    public class TradeOrderDTO
    {
		public int Id { get; set; }
		public int TradeActionId { get; set; }
		public string TradeActionName { get; set; }
		public short TradeOrderStatusId { get; set; }
		public string TradeOrderStatusName { get; set; }
		public int? TradeRuleId { get; set; }
		public string TradeRuleName { get; set; }
		public Guid OrderId { get; set; }
		public DateTime OrderDateTime { get; set; }
		public decimal Price { get; set; }
		public decimal Amount { get; set; }
		public decimal FilledAmount { get; set; }
		public bool IsTestOrder { get; set; }
		public bool IsActive { get; set; }

		public override string ToString()
        {
			return $"{OrderId} {OrderDateTime} {Price} {FilledAmount}/{Amount}";
        }
    }
}
