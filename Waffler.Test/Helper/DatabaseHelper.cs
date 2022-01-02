using System;

using Microsoft.EntityFrameworkCore;

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

            foreach (Variable.CandleStickValueType candleStickValueType in Enum.GetValues(typeof(Variable.CandleStickValueType)))
            {
                context.CandleStickValueTypes.Add(new CandleStickValueType()
                {
                    Id = (short)candleStickValueType,
                    Name = candleStickValueType.ToString()
                });
            }
            
            foreach (Variable.TradeType tradeType in Enum.GetValues(typeof(Variable.TradeType)))
            {
                context.TradeTypes.Add(new TradeType()
                {
                    Id = (short)tradeType,
                    Name = tradeType.ToString()
                });
            }

            foreach (Variable.TradeConditionOperator tradeConditionOperator in Enum.GetValues(typeof(Variable.TradeConditionOperator)))
            {
                context.TradeConditionOperators.Add(new TradeConditionOperator()
                {
                    Id = (short)tradeConditionOperator,
                    Name = tradeConditionOperator.ToString()
                });
            }

            foreach (Variable.TradeRuleStatus tradeRuleStatus in Enum.GetValues(typeof(Variable.TradeRuleStatus)))
            {
                context.TradeRuleStatuses.Add(new TradeRuleStatus()
                {
                    Id = (short)tradeRuleStatus,
                    Name = tradeRuleStatus.ToString()
                });
            }

            foreach (Variable.TradeRuleConditionComparator tradeRuleConditionComparator in Enum.GetValues(typeof(Variable.TradeRuleConditionComparator)))
            {
                context.TradeRuleConditionComparators.Add(new TradeRuleConditionComparator()
                {
                    Id = (short)tradeRuleConditionComparator,
                    Name = tradeRuleConditionComparator.ToString()
                });
            }

            foreach (Variable.TradeRuleConditionPeriodDirection tradeRuleConditionPeriodDirection in Enum.GetValues(typeof(Variable.TradeRuleConditionPeriodDirection)))
            {
                context.TradeRuleConditionPeriodDirections.Add(new TradeRuleConditionPeriodDirection()
                {
                    Id = (short)tradeRuleConditionPeriodDirection,
                    Name = tradeRuleConditionPeriodDirection.ToString()
                });
            }
        }
    }
}
