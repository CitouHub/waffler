using System.Collections.Generic;
using static Waffler.Common.Variable;

namespace Waffler.Common
{
    public static class Bitpanda
    {
        private static class InstrumentCode
        {
            public const string BTC_EUR = "BTC_EUR";
        }

        public static class Period
        {
            public const string MINUTES = "MINUTES";
        }

        public static string GetInstrumentCode(TradeType tradeType)
        {
            return tradeType switch
            {
                TradeType.BTC_EUR => InstrumentCode.BTC_EUR,
                _ => null,
            };
        }

        public static TradeType GetTradeType(string instrumentCode)
        {
            return instrumentCode switch
            {
                InstrumentCode.BTC_EUR => TradeType.BTC_EUR,
                _ => default,
            };
        }
    }
}
