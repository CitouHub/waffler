using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using AutoMapper;

using Waffler.Domain;
using Waffler.Data;
using Waffler.Common;
using Waffler.Domain.Export;

namespace Waffler.Service
{
    public interface ITradeRuleService
    {
        Task<TradeRuleDTO> NewTradeRuleAsync();

        Task<bool> AddTradeRuleAsync(TradeRuleDTO tradeRule);

        Task<bool> CopyTradeRuleAsync(int tradeRuleId);

        Task<List<TradeRuleDTO>> GetTradeRulesAsync();

        Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleAttributesAsync();

        Task<TradeRuleDTO> GetTradeRuleAsync(int tradeRuleId);

        Task<TradeRuleExportDTO> GetTradeRuleAsyncForExport(int tradeRuleId);

        Task<bool> UpdateTradeRuleAsync(TradeRuleDTO tradeRuleDTO);

        Task<bool> SetupTradeRuleTestAsync(int tradeRuleId);

        Task<bool> DeleteTradeRuleAsync(int tradeRuleId);
    }

    public class TradeRuleService : ITradeRuleService
    {
        private readonly ILogger<TradeRuleService> _logger;
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;

        public TradeRuleService(ILogger<TradeRuleService> logger, WafflerDbContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _logger.LogDebug("TradeRuleService instantiated");
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
                CandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice,
                Name = "New trade rule",
                Amount = 0,
                TradeMinIntervalMinutes = (int)TimeSpan.FromDays(1).TotalMinutes
            };

            await _context.TradeRules.AddAsync(newTradeRule);
            await _context.SaveChangesAsync();

            return _mapper.Map<TradeRuleDTO>(newTradeRule);
        }

        public async Task<bool> AddTradeRuleAsync(TradeRuleDTO tradeRule)
        {
            var newTradeRule = _mapper.Map<TradeRule>(tradeRule);
            newTradeRule.Id = 0;
            newTradeRule.InsertDate = DateTime.UtcNow;
            newTradeRule.InsertByUser = 1;
            newTradeRule.Name = $"{newTradeRule.Name} (Imported)";

            await _context.TradeRules.AddAsync(newTradeRule);
            await _context.SaveChangesAsync();

            foreach (var tradeRuleCondition in tradeRule.TradeRuleConditions)
            {
                var newTradeRuleCondition = _mapper.Map<TradeRuleCondition>(tradeRuleCondition);
                newTradeRuleCondition.Id = 0;
                newTradeRuleCondition.InsertDate = DateTime.UtcNow;
                newTradeRuleCondition.InsertByUser = 1;
                newTradeRuleCondition.TradeRuleId = newTradeRule.Id;

                await _context.TradeRuleConditions.AddAsync(newTradeRuleCondition);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CopyTradeRuleAsync(int tradeRuleId)
        {
            var tradeRule = await _context.TradeRules
                .Include(_ => _.TradeRuleConditions)
                .FirstOrDefaultAsync(_ => _.Id == tradeRuleId && _.IsDeleted == false);
            if (tradeRule != null)
            {
                var copyTradeRule = _mapper.Map<TradeRuleDTO>(tradeRule);
                var newTradeRule = _mapper.Map<TradeRule>(copyTradeRule);
                newTradeRule.Id = 0;
                newTradeRule.InsertDate = DateTime.UtcNow;
                newTradeRule.InsertByUser = 1;
                newTradeRule.Name = $"{newTradeRule.Name} (Copy)";

                await _context.TradeRules.AddAsync(newTradeRule);
                await _context.SaveChangesAsync();

                foreach (var tradeRuleCondition in tradeRule.TradeRuleConditions)
                {
                    var copyTradeRuleCondition = _mapper.Map<TradeRuleConditionDTO>(tradeRuleCondition);
                    var newTradeRuleCondition = _mapper.Map<TradeRuleCondition>(copyTradeRuleCondition);
                    newTradeRuleCondition.Id = 0;
                    newTradeRuleCondition.InsertDate = DateTime.UtcNow;
                    newTradeRuleCondition.InsertByUser = 1;
                    newTradeRuleCondition.TradeRuleId = newTradeRule.Id;

                    await _context.TradeRuleConditions.AddAsync(newTradeRuleCondition);
                }

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<List<TradeRuleDTO>> GetTradeRulesAsync()
        {
            var tradeRules = await _context.TradeRules
                .Include(_ => _.TradeAction)
                .Include(_ => _.TradeType)
                .Include(_ => _.TradeConditionOperator)
                .Include(_ => _.TradeRuleStatus)
                .Include(_ => _.TradeRuleConditions)
                .Include(_ => _.CandleStickValueType)
                .Where(_ => _.IsDeleted == false)
                .ToListAsync();
            return _mapper.Map<List<TradeRuleDTO>>(tradeRules);
        }

        public async Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleAttributesAsync()
        {
            return new Dictionary<string, List<CommonAttributeDTO>>()
            {
                {
                    nameof(TradeAction),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeActions.ToArrayAsync())
                },
                {
                    nameof(TradeType),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeTypes.ToArrayAsync())
                },
                {
                    nameof(TradeConditionOperator),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeConditionOperators.ToArrayAsync())
                },
                {
                    nameof(TradeRuleStatus),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeRuleStatuses.ToArrayAsync())
                },
                {
                    nameof(CandleStickValueType),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.CandleStickValueTypes.ToArrayAsync())
                }
            };
        }

        public async Task<TradeRuleDTO> GetTradeRuleAsync(int tradeRuleId)
        {
            var tradeRule = await _context.TradeRules
                .Include(_ => _.TradeRuleConditions)
                .FirstOrDefaultAsync(_ => _.Id == tradeRuleId && _.IsDeleted == false);
            return _mapper.Map<TradeRuleDTO>(tradeRule);
        }

        public async Task<TradeRuleExportDTO> GetTradeRuleAsyncForExport(int tradeRuleId)
        {
            var tradeRule = await GetTradeRuleAsync(tradeRuleId);
            return _mapper.Map<TradeRuleExportDTO>(tradeRule);
        }

        public async Task<bool> SetupTradeRuleTestAsync(int tradeRuleId)
        {
            var tradeRule = await _context.TradeRules.FindAsync(tradeRuleId);
            if (tradeRule != null)
            {
                tradeRule.LastTrigger = DateTime.MinValue;
                tradeRule.TradeRuleStatusId = (short)Variable.TradeRuleStatus.Test;
                tradeRule.UpdateByUser = 1;
                tradeRule.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> UpdateTradeRuleAsync(TradeRuleDTO tradeRuleDTO)
        {
            var tradeRule = await _context.TradeRules.FindAsync(tradeRuleDTO.Id);
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
            var tradeRule = await _context.TradeRules.FindAsync(tradeRuleId);
            if (tradeRule != null)
            {
                tradeRule.IsDeleted = true;
                tradeRule.UpdateByUser = 1;
                tradeRule.UpdateDate = DateTime.UtcNow;

                var tradeOrders = _context.TradeOrders.Where(_ => 
                    _.TradeRuleId == tradeRule.Id && 
                    _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test);
                _context.TradeOrders.RemoveRange(tradeOrders);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}