namespace Waffler.Common
{
    public class Variable
    {
        public enum CandleStickValueType
        {
            HighPrice = 1,
            LowPrice = 2,
            OpenPrice = 3,
            ClosePrice = 4,
            HighLowPrice = 5,
            OpenClosePrice = 6,
            AvgHighLowPrice = 7,
            AvgOpenClosePrice = 8
        }

        public enum TradeRuleConditionComparator
        {
            LessThen = 1,
            MoreThen = 2,
            AbsLessThen = 3,
            AbsMoreThen = 4
        }

        public enum TradeRuleStatus
        {
            Active = 1,
            Test = 2,
            Disabled = 3
        }

        public enum TradeRuleConditionSampleDirection
        {
            Centered = 1,
            LeftShift = 2,
            RightShift = 3,
        }

        public enum TradeConditionOperator
        {
            AND = 1,
            OR = 2
        }

        public enum TradeAction
        {
            Buy = 1,
            Sell = 2
        }

        public enum TradeType
        {
            BTC_EUR = 1
        }

        public enum CurrencyCode
        {
            BTC = 1,
            EUR = 2
        }

        public enum TradeOrderStatus
        {
            Open = 1,
            StopTriggered = 2,
            Filled = 3,
            FilledFully = 4,
            FilledClosed = 5,
            FilledRejected = 6,
            Rejected = 7,
            Closed = 8,
            Failed = 9
        }
    }
}
