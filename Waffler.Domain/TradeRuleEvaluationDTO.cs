using System.Collections.Generic;
using System.Linq;

namespace Waffler.Domain
{
    public class TradeRuleEvaluationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TradeRuleConditionEvaluationDTO> TradeRuleCondtionEvaluations;

        public override string ToString()
        {
            return $"Rule: \"{Name}\", result: {string.Join(", ", TradeRuleCondtionEvaluations.Select(_ => $"{_.Description}: {_.IsFullfilled}"))}";
        }
    }
}