using System;

#pragma warning disable IDE1006 // Naming Styles
namespace Waffler.Data.ComplexModel
{
    public class sp_getTradeOrders_Result
	{
		public int Id { get; set; }
		public int TradeRuleId { get; set; }
		public string TradeRuleName { get; set; }
		public short TradeActionId { get; set; }
		public string TradeActionName { get; set; }
		public short TradeOrderStatusId { get; set; }
		public string TradeOrderStatusName { get; set; }
		public Guid OrderId { get; set; }
		public DateTime OrderDateTime { get; set; }
		public decimal Price { get; set; }
		public decimal Amount { get; set; }
		public decimal FilledAmount { get; set; }
	}
}
