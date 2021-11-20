﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Waffler.Domain;
using Waffler.Data;
using Waffler.Common;

namespace Waffler.Service
{
    public interface ITradeRuleService
    {
        Task<TradeRuleDTO> NewTradeRuleAsync();

        Task<List<TradeRuleDTO>> GetTradeRulesAsync();

        Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleAttributesAsync();

        Task<TradeRuleDTO> GetTradeRuleAsync(int tradeRuleId);

        Task<bool> UpdateTradeRuleAsync(TradeRuleDTO tradeRuleDTO);

        Task<bool> UpdateTradeRuleLastTriggerAsync(int tradeRuleId, DateTime triggerTime);

        Task<bool> SetupTradeRuleTestAsync(int tradeRuleId);

        Task<bool> DeleteTradeRuleAsync(int tradeRuleId);
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

        public async Task<TradeRuleDTO> NewTradeRuleAsync()
        {
            var newTradeRule = new TradeRule()
            {
                InsertDate = DateTime.UtcNow,
                InsertByUser = 1,
                TradeActionId = (short)Variable.TradeAction.Buy,
                TradeTypeId = (short)Variable.TradeType.BTC_EUR,
                TradeConditionOperatorId = (short)Variable.TradeConditionOperator.AND,
                Name = "New trade rule",
                Amount = 0,
                TradeMinIntervalMinutes = (int)TimeSpan.FromDays(1).TotalMinutes
            };

            await _context.TradeRule.AddAsync(newTradeRule);
            await _context.SaveChangesAsync();

            return _mapper.Map<TradeRuleDTO>(newTradeRule);
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

        public async Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleAttributesAsync()
        {
            return new Dictionary<string, List<CommonAttributeDTO>>()
            {
                {
                    nameof(TradeAction),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeAction.ToArrayAsync())
                },
                                {
                    nameof(TradeType),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeType.ToArrayAsync())
                },
                                                {
                    nameof(TradeConditionOperator),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeConditionOperator.ToArrayAsync())
                },
                                                {
                    nameof(TradeRuleStatus),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeRuleStatus.ToArrayAsync())
                }
            };
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

        public async Task<bool> UpdateTradeRuleLastTriggerAsync(int tradeRuleId, DateTime triggerTime)
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
                tradeRule.TradeRuleStatusId = (short)Variable.TradeRuleStatus.Test;
                tradeRule.TestTradeInProgress = true;
                tradeRule.UpdateByUser = 1;
                tradeRule.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> UpdateTradeRuleAsync(TradeRuleDTO tradeRuleDTO)
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

        public async Task<bool> DeleteTradeRuleAsync(int tradeRuleId)
        {
            var tradeRule = await _context.TradeRule.FindAsync(tradeRuleId);
            if(tradeRule != null)
            {
                _context.TradeRule.Remove(tradeRule);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
