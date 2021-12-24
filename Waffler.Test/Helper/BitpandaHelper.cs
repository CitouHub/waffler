using System;

using Waffler.Common;
using Waffler.Domain.Bitpanda.Private.Order;

namespace Waffler.Test.Helper
{
    public static class BitpandaHelper
    {
        public static OrderSubmittedDTO GetOrderSubmitted()
        {
            return new OrderSubmittedDTO()
            {
                Account_id = Guid.NewGuid().ToString(),
                Amount = 0,
                Client_id = Guid.NewGuid().ToString(),
                Filled_amount = 0,
                Instrument_code = Bitpanda.InstrumentCode.BTC_EUR,
                Order_id = Guid.NewGuid().ToString(),
                Price = 0,
                Side = Bitpanda.Side.BUY,
                Time = DateTime.UtcNow,
                Type = Bitpanda.OrderType.LIMIT
            };
        }
    }
}
