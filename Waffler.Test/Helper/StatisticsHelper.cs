using Waffler.Domain.Statistics;

namespace Waffler.Test.Helper
{
    public static class StatisticsHelper
    {
        public static TrendDTO GetTrendDTO(decimal change)
        {
            return new TrendDTO()
            {
                FromPrice = 1000,
                ToPrice = 1000 + (1000 * (change / 100))
            };
        }
    }
}
