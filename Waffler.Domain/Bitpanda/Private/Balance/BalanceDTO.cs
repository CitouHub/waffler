using System;

namespace Waffler.Domain.Bitpanda.Private.Balance
{
    public class BalanceDTO
    {
        public string account_id { get; set; }
        public string currency_code { get; set; }
        public decimal change { get; set; }
        public decimal available { get; set; }
        public decimal locked { get; set; }
        public long sequence { get; set; }
        public DateTime time { get; set; }
    }
}
