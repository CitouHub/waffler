using System.Threading;

namespace Waffler.Domain
{
    public class TradeRuleTestRunDTO
    {
        public TradeRuleTestStatusDTO TradeRuleTestStatus { get; set; }

        public bool Abort { get; set; }

        public SemaphoreSlim CloseSignal { get; set; }
    }
}