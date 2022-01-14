using System;

namespace Waffler.Domain.ComplexMapping
{
    public static class BalanceMapper
    {
        private static readonly short BalanceEURPrecision = 2;
        private static readonly short BalanceBTCPrecision = 8;

        public static decimal RoundBalance(string currencyCode, decimal value)
        {
            return currencyCode switch
            {
                Common.Bitpanda.CurrencyCode.BTC => Math.Round(value, BalanceBTCPrecision),
                Common.Bitpanda.CurrencyCode.EUR => Math.Round(value, BalanceEURPrecision),
                _ => value,
            };
        }
    }
}