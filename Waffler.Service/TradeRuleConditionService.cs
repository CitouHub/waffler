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

namespace Waffler.Service
{
    public interface ITradeRuleConditionService
    {
        Task<TradeRuleConditionDTO> NewTradeRuleConditionAsync(int tradeRuleId);

        Task<List<TradeRuleConditionDTO>> GetTradeRuleConditionsAsync(int tradeRuleId);

        Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleConditionAttributesAsync();

        Task<bool> UpdateTradeRuleConditionAsync(TradeRuleConditionDTO tradeRuleConditionDTO);

        Task<bool> DeleteTradeRuleConditionAsync(int tradeRuleId);
    }

    public class TradeRuleConditionService : ITradeRuleConditionService
    {
        private readonly ILogger<TradeRuleConditionService> _logger;
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;

        public TradeRuleConditionService(ILogger<TradeRuleConditionService> logger, WafflerDbContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _logger.LogDebug("TradeRuleConditionService instantiated");
        }

        public async Task<TradeRuleConditionDTO> NewTradeRuleConditionAsync(int tradeRuleId)
        {
            var newTradeRuleCondition = new TradeRuleCondition()
            {
                InsertDate = DateTime.UtcNow,
                InsertByUser = 1,
                TradeRuleId = tradeRuleId,
                TradeRuleConditionComparatorId = (short)Variable.TradeRuleConditionComparator.LessThen,

                FromCandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice,
                FromTradeRuleConditionPeriodDirectionId = (short)Variable.TradeRuleConditionPeriodDirection.Centered,
                FromMinutes = 0,
                FromPeriodMinutes = 0,

                ToCandleStickValueTypeId = (short)Variable.CandleStickValueType.HighPrice,
                ToTradeRuleConditionPeriodDirectionId = (short)Variable.TradeRuleConditionPeriodDirection.LeftShift,
                ToMinutes = 0,
                ToPeriodMinutes = 0,

                DeltaPercent = 0,
                Description = "New condition",
                IsOn = false
            };

            await _context.TradeRuleConditions.AddAsync(newTradeRuleCondition);
            await _context.SaveChangesAsync();

            return _mapper.Map<TradeRuleConditionDTO>(newTradeRuleCondition);
        }

        public async Task<List<TradeRuleConditionDTO>> GetTradeRuleConditionsAsync(int tradeRuleId)
        {
            var tradeRuleConditions = await _context.TradeRuleConditions
                .Include(_ => _.TradeRuleConditionComparator)
                .Include(_ => _.FromCandleStickValueType)
                .Include(_ => _.FromTradeRuleConditionPeriodDirection)
                .Include(_ => _.ToCandleStickValueType)
                .Include(_ => _.ToTradeRuleConditionPeriodDirection)
                .Where(_ => _.TradeRuleId == tradeRuleId)
                .ToListAsync();
            return _mapper.Map<List<TradeRuleConditionDTO>>(tradeRuleConditions);
        }

        public async Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleConditionAttributesAsync()
        {
            return new Dictionary<string, List<CommonAttributeDTO>>()
            {
                {
                    nameof(CandleStickValueType),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.CandleStickValueTypes.ToArrayAsync())
                },
                {
                    nameof(TradeRuleConditionComparator),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeRuleConditionComparators.ToArrayAsync())
                },
                {
                    nameof(TradeRuleConditionPeriodDirection),
                    _mapper.Map<List<CommonAttributeDTO>>(await _context.TradeRuleConditionPeriodDirections.ToArrayAsync())
                }
            };
        }

        public async Task<bool> UpdateTradeRuleConditionAsync(TradeRuleConditionDTO tradeRuleConditionDTO)
        {
            var tradeRuleCondition = await _context.TradeRuleConditions.FindAsync(tradeRuleConditionDTO.Id);
            if (tradeRuleCondition != null)
            {
                _mapper.Map(tradeRuleConditionDTO, tradeRuleCondition);
                tradeRuleCondition.UpdateByUser = 1;
                tradeRuleCondition.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteTradeRuleConditionAsync(int tradeRuleConditionId)
        {
            var tradeRuleCondition = await _context.TradeRuleConditions.FindAsync(tradeRuleConditionId);
            if(tradeRuleCondition != null)
            {
                _context.TradeRuleConditions.Remove(tradeRuleCondition);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
