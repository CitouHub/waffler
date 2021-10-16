using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using WaffleBot.Data.ComplexModel;

namespace WaffleBot.Data.Extensions
{
    public static class StoredProcedure
    {
        public static async Task<List<sp_getPriceTrends_Result>> sp_getPriceTrends(this WafflerDbContext context,
            DateTime fromFromDateTime,
	        DateTime fromToDateTime,
	        DateTime toFromDateTime,
	        DateTime toToDateTime)
        {
            var expr = $"exec sp_getPriceTrends " +
                $"'{fromFromDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"'{fromToDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"'{toFromDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"'{toToDateTime:yyyy-MM-dd HH:mm:ss}'";

            return await context.Set<sp_getPriceTrends_Result>().FromSqlRaw(expr).ToListAsync();
        }
    }
}