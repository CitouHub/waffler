using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using WaffleBot.Domain;
using WaffleBot.Data;

namespace WaffleBot.Service
{
    public interface ITradeRuleService
    {
        Task<List<TradeRuleDTO>> GetTradeRulesAsync();

        Task<bool> UpdateTradeRuleLastTrigger(int tradeRuleId, DateTime triggerTime);
    }

    public class TradeRuleService : ITradeRuleService
    {
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;

        public TradeRuleService(WafflerDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TradeRuleDTO>> GetTradeRulesAsync()
        {
            var tradeRules = await _context.TradeRule
                .Include(_ => _.TradeRuleCondition).ToListAsync();
            return _mapper.Map<List<TradeRuleDTO>>(tradeRules);
        }

        public async Task<bool> UpdateTradeRuleLastTrigger(int tradeRuleId, DateTime triggerTime)
        {
            var tradeRule = await _context.TradeRule.FindAsync(tradeRuleId);
            if(tradeRule != null)
            {
                tradeRule.LastTrigger = triggerTime;
                tradeRule.UpdateByUser = 1;
                tradeRule.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
