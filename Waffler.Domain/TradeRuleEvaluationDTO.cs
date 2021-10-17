﻿using System.Collections.Generic;
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
            return $"Rule: {Id}:{Name}, result: {string.Join(", ", TradeRuleCondtionEvaluations.Select(_ => $"{_.Id}:{_.Description}:{_.IsFullfilled}"))}";
        }
    }
}