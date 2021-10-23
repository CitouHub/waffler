using static Waffler.Common.Variable;

namespace Waffler.Domain
{
    public class BalanceDTO
    {
        public CurrencyCode CurrencyCode { get; set; }
        public decimal Available { get; set; }
        public decimal TradeLocked { get; set; }
    }
}