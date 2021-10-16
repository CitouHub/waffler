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
            DateTime startTime,
            int fromMinutesOffset, 
            int toMinutesOffset, 
            int fromMinutesSample, 
            int toMinutesSample)
        {
            return await context.Set<sp_getPriceTrends_Result>().FromSqlRaw($"exec sp_getPriceTrends " +
                $"'{startTime.ToString("yyyy-MM-dd hh:mm:ss")}'" + 
                $"{fromMinutesOffset}, " +
                $"{toMinutesOffset}, " +
                $"{fromMinutesSample}, " +
                $"{toMinutesSample}").ToListAsync();
        }
    }
}