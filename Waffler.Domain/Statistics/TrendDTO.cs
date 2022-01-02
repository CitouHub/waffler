using System;

namespace Waffler.Domain.Statistics
{
    public class TrendDTO
    {
        public decimal FromPrice { get; set; }
        public decimal ToPrice { get; set; }
        public decimal Change
        {
            get
            {
                var change = (decimal)0.0;
                if (FromPrice != 0)
                {
                    change = Math.Round((ToPrice / FromPrice - 1) * 100, 2);
                }

                return change;
            }
        }       
    }
}