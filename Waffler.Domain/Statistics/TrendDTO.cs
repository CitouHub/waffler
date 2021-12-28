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
                if(FromPrice == 0)
                {
                    return 0;
                }

                return Math.Round((ToPrice / FromPrice - 1) * 100, 2);
            }
        }
    }
}
