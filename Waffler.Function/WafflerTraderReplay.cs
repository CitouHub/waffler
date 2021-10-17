using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waffler.Domain;

namespace Waffler.Function
{
    public class WafflerTraderReplay
    {
        [FunctionName("WafflerTradeAnalyserReplay2")]
        public async Task RunWafflerTradeAnalyserReplay2Async([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            if (1==1)
            {
                var toPeriodDateTime = new DateTime(2021, 10, 6, 0, 0, 0, DateTimeKind.Utc);
                //var toPeriodDateTime = new DateTime(2021, 10, 13, 11, 45, 0, DateTimeKind.Utc);
                var results = new List<TradeRuleEvaluationDTO>();
                //while (toPeriodDateTime < DateTime.UtcNow)
                //{
                //    var result = await HandleTradeRules(toPeriodDateTime, log);
                //    results.AddRange(result);
                //    toPeriodDateTime = toPeriodDateTime.AddMinutes(15);
                //}

                foreach (var tradeRule in results.GroupBy(_ => new { _.Id, _.Name }))
                {
                    log.LogInformation($"Trade rule result: {tradeRule.Key.Id}:{tradeRule.Key.Name}");
                    var tradeRuleConditions = results.Where(_ => _.Id == tradeRule.Key.Id)
                        .SelectMany(_ => _.TradeRuleCondtionEvaluations)
                        .Where(_ => _.IsFullfilled == true)
                        .GroupBy(_ => new { _.Id, _.Description })
                        .Select(_ => new { _.Key.Id, _.Key.Description, Count = _.Count() });
                    foreach (var tradeRuleCondition in tradeRuleConditions)
                    {
                        log.LogInformation($"- Condition: {tradeRuleCondition.Id}:{tradeRuleCondition.Description} = {tradeRuleCondition.Count}");
                    }
                }
            }
        }
    }
}
