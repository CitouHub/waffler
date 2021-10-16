using static WaffleBot.Common.Variable;

namespace WaffleBot.Common
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
            switch(tradeType)
            {
                case TradeType.BTC_EUR:
                    return InstrumentCode.BTC_EUR;
                default:
                    return null;
            }
        }
    }
}
