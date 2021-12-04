using System;

namespace Waffler.Domain.Bitpanda.Private.Balance
{
    public class BalanceDTO
    {
        public string Account_id { get; set; }
        public string Currency_code { get; set; }
        public decimal Change { get; set; }
        public decimal Available { get; set; }
        public decimal Locked { get; set; }
        public long Sequence { get; set; }
        public DateTime Time { get; set; }
    }
}
