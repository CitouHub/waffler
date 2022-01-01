using Microsoft.EntityFrameworkCore;
using System;
using Waffler.Common;
using Waffler.Data;

namespace Waffler.Test.Helper
{
    public static class DatabaseHelper
    {
        public static WafflerDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<BaseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new WafflerDbContext(options);
            AddDefaults(context);

            return context;
        }

        private static void AddDefaults(WafflerDbContext context)
        {
            foreach (Variable.TradeAction tradeAction in Enum.GetValues(typeof(Variable.TradeAction)))
            {
                context.TradeActions.Add(new TradeAction()
                {
                    Id = (short)tradeAction,
                    Name = tradeAction.ToString()
                });
            }

            foreach (Variable.TradeOrderStatus tradeOrderStatus in Enum.GetValues(typeof(Variable.TradeOrderStatus)))
            {
                context.TradeOrderStatuses.Add(new TradeOrderStatus()
                {
                    Id = (short)tradeOrderStatus,
                    Name = tradeOrderStatus.ToString()
                });
            }
        }
    }
}
