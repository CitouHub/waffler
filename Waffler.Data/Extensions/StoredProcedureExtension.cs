using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Waffler.Data.ComplexModel;

#pragma warning disable IDE1006 // Naming Styles
namespace Waffler.Data.Extensions
{
    public static class StoredProcedureExtension
    {
        public static async Task<List<sp_getCandleSticks_Result>> sp_getCandleSticks(this WafflerDbContext context,
            DateTime fromPeriodDateTime, 
            DateTime toPeriodDateTime, 
            short tradeTypeId, 
            short periodDateTimeGroup)
        {
            var expr = $"exec sp_getCandleSticks " +
                $"'{fromPeriodDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"'{toPeriodDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"{ tradeTypeId}, " +
                $"{ periodDateTimeGroup}";

            return await context.Set<sp_getCandleSticks_Result>().FromSqlRaw(expr).ToListAsync();
        }

        public static async Task<List<sp_getTradeOrders_Result>> sp_getTradeOrders(this WafflerDbContext context,
            DateTime fromPeriodDateTime,
            DateTime toPeriodDateTime)
        {
            var expr = $"exec sp_getTradeOrders " +
                $"'{fromPeriodDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"'{toPeriodDateTime:yyyy-MM-dd HH:mm:ss}'";

            return await context.Set<sp_getTradeOrders_Result>().FromSqlRaw(expr).ToListAsync();
        }

        public static async Task<List<sp_getBuyTradeRuleStatistics_Result>> sp_getBuyTradeRuleStatistics(this WafflerDbContext context,
            DateTime fromPeriodDateTime,
            DateTime toPeriodDateTime)
        {
            var expr = $"exec sp_getBuyTradeRuleStatistics " +
                $"'{fromPeriodDateTime:yyyy-MM-dd HH:mm:ss}', " +
                $"'{toPeriodDateTime:yyyy-MM-dd HH:mm:ss}'";

            return await context.Set<sp_getBuyTradeRuleStatistics_Result>().FromSqlRaw(expr).ToListAsync();
        }
    }
}