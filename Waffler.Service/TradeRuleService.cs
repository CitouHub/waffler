using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Waffler.Domain;
using Waffler.Data;

namespace Waffler.Service
{
    public interface ITradeRuleService
    {
        Task<List<TradeRuleDTO>> GetTradeRulesAsync();

        Task<TradeRuleDTO> GetTradeRuleAsync(int tradeRuleId);

        Task<bool> UpdateTradeRuleLastTrigger(int tradeRuleId, DateTime triggerTime);

        Task<bool> SetupTradeRuleTestAsync(int tradeRuleId);

        Task<bool> UpdateTradeRule(TradeRuleDTO tradeRuleDTO);
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
                .Include(_ => _.TradeAction)
                .Include(_ => _.TradeType)
                .Include(_ => _.TradeConditionOperator)
                .Include(_ => _.TradeRuleStatus)
                .Include(_ => _.TradeRuleCondition).ThenInclude(_ => _.CandleStickValueType)
                .Include(_ => _.TradeRuleCondition).ThenInclude(_ => _.TradeRuleConditionComparator)
                .Include(_ => _.TradeRuleCondition).ThenInclude(_ => _.TradeRuleConditionSampleDirection)
                .ToListAsync();
            return _mapper.Map<List<TradeRuleDTO>>(tradeRules);
        }

        public async Task<TradeRuleDTO> GetTradeRuleAsync(int tradeRuleId)
        {
            var tradeRule = await _context.TradeRule
                .Include(_ => _.TradeAction)
                .Include(_ => _.TradeType)
                .Include(_ => _.TradeConditionOperator)
                .Include(_ => _.TradeRuleCondition).ThenInclude(_ => _.CandleStickValueType)
                .Include(_ => _.TradeRuleCondition).ThenInclude(_ => _.TradeRuleConditionComparator)
                .Include(_ => _.TradeRuleCondition).ThenInclude(_ => _.TradeRuleConditionSampleDirection)
                .FirstOrDefaultAsync(_ => _.Id == tradeRuleId);
            return _mapper.Map<TradeRuleDTO>(tradeRule);
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

        public async Task<bool> SetupTradeRuleTestAsync(int tradeRuleId)
        {
            var tradeRule = await _context.TradeRule.FindAsync(tradeRuleId);
            if (tradeRule != null)
            {
                tradeRule.LastTrigger = DateTime.MinValue;
                tradeRule.TradeRuleStatusId = (short)Common.Variable.TradeRuleStatus.Test;
                tradeRule.QueuedForTestTrade = true;
                tradeRule.UpdateByUser = 1;
                tradeRule.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> UpdateTradeRule(TradeRuleDTO tradeRuleDTO)
        {
            var tradeRule = await _context.TradeRule.FindAsync(tradeRuleDTO.Id);
            if (tradeRule != null)
            {
                _mapper.Map(tradeRuleDTO, tradeRule);
                tradeRule.UpdateByUser = 1;
                tradeRule.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
