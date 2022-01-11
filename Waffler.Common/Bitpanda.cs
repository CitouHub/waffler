using static Waffler.Common.Variable;

namespace Waffler.Common
{
    public static class Bitpanda
    {
        public static short PriceDecimalPrecision = 2;
        public static short AmountDecimalPrecision = 5;

        public static class InstrumentCode
        {
            public const string BTC_EUR = "BTC_EUR";
        }

        public static class Period
        {
            public const string MINUTES = "MINUTES";
            public const string HOURS = "HOURS";
        }

        public static class CurrencyCode
        {
            public const string BTC = "BTC";
            public const string EUR = "EUR";
        }

        public static class OrderType
        {
            public const string LIMIT = "LIMIT";
            public const string STOP = "STOP";
            public const string MARKET = "MARKET";
        }

        public static class TimeInForce
        {
            public const string GOOD_TILL_CANCELLED = "GOOD_TILL_CANCELLED";
            public const string GOOD_TILL_TIME = "GOOD_TILL_TIME";
            public const string IMMEDIATE_OR_CANCELLED = "IMMEDIATE_OR_CANCELLED";
            public const string FILL_OR_KILL = "FILL_OR_KILL";
        }

        public static class Side
        {
            public const string BUY = "BUY";
            public const string SELL = "SELL";
        }

        public static class Status
        {
            public const string OPEN = "OPEN";
            public const string STOP_TRIGGERED = "STOP_TRIGGERED";
            public const string FILLED = "FILLED";
            public const string FILLED_FULLY = "FILLED_FULLY";
            public const string FILLED_CLOSED = "FILLED_CLOSED";
            public const string FILLED_REJECTED = "FILLED_REJECTED";
            public const string REJECTED = "REJECTED";
            public const string CLOSED = "CLOSED";
            public const string FAILED = "FAILED";
        }

        public static string GetInstrumentCode(TradeType tradeType)
        {
            return tradeType switch
            {
                TradeType.BTC_EUR => InstrumentCode.BTC_EUR,
                _ => null,
            };
        }

        public static string GetSide(TradeAction tradeAction)
        {
            return tradeAction switch
            {
                TradeAction.Buy => Side.BUY,
                TradeAction.Sell => Side.SELL,
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

        public static Variable.CurrencyCode GetCurrentCode(string currencyCode)
        {
            return currencyCode switch
            {
                CurrencyCode.BTC => Variable.CurrencyCode.BTC,
                CurrencyCode.EUR => Variable.CurrencyCode.EUR,
                _ => default,
            };
        }

        public static TradeAction GetTradeAction(string side)
        {
            return side switch
            {
                Side.BUY => TradeAction.Buy,
                Side.SELL => TradeAction.Sell,
                _ => default,
            };
        }

        public static TradeOrderStatus GetTradeOrderStatus(string status)
        {
            return status switch
            {
                Status.OPEN => TradeOrderStatus.Open,
                Status.STOP_TRIGGERED => TradeOrderStatus.StopTriggered,
                Status.FILLED => TradeOrderStatus.Filled,
                Status.FILLED_FULLY => TradeOrderStatus.FilledFully,
                Status.FILLED_CLOSED => TradeOrderStatus.FilledClosed,
                Status.FILLED_REJECTED => TradeOrderStatus.FilledRejected,
                Status.REJECTED => TradeOrderStatus.Rejected,
                Status.CLOSED => TradeOrderStatus.Closed,
                Status.FAILED => TradeOrderStatus.Failed,
                _ => default,
            };
        }

        public static bool GetTradeOrderActive(string status)
        {
            return status switch
            {
                Status.OPEN => true,
                Status.STOP_TRIGGERED => false,
                Status.FILLED => true,
                Status.FILLED_FULLY => false,
                Status.FILLED_CLOSED => false,
                Status.FILLED_REJECTED => false,
                Status.REJECTED => false,
                Status.CLOSED => false,
                Status.FAILED => false,
                _ => default,
            };
        }
    }
}
