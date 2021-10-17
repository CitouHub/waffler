using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Waffler.Data.ComplexModel;

namespace Waffler.Data.Extensions
{
    public static class StoredProcedure
    {
        public static async Task<List<sp_getPriceStatistics_Result>> sp_getPriceStatistics(this WafflerDbContext context,
            short candleStickValueTypeId,
            short periodDateTimeGroup,
            DateTime fromPeriodDateTime,
	        DateTime toPeriodDateTime)
        {
            var expr = $"exec sp_getPriceStatistics " +
                $"{candleStickValueTypeId}, " +
                $"{periodDateTimeGroup}, " +
                $"'{fromPeriodDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"'{toPeriodDateTime:yyyy-MM-dd HH:mm:ss}'";

            return await context.Set<sp_getPriceStatistics_Result>().FromSqlRaw(expr).ToListAsync();
        }
    }
}