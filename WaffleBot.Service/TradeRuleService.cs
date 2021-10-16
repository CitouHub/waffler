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
            var tradeRules = await _context.TradeRule.Include(_ => _.TradeRuleCondition).ToListAsync();
            return _mapper.Map<List<TradeRuleDTO>>(tradeRules);
        }
    }
}
