using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Waffler.Data;
using Waffler.Domain;
using Waffler.Data.Extensions;

namespace Waffler.Service
{
    public interface ITradeOrderService
    {
        Task CreateTradeOrder(TradeOrderDTO tradeOrderDto);
        Task<List<TradeOrderDTO>> GetTradeOrders(DateTime from, DateTime to);
        Task RemoveTestTradeOrders(int tradeRuleId);
    }

    public class TradeOrderService : ITradeOrderService
    {
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;

        public TradeOrderService(WafflerDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TradeOrderDTO>> GetTradeOrders(DateTime from, DateTime to)
        {
            var tradeOrders = await _context.sp_getTradeOrders(from, to);
            return _mapper.Map<List<TradeOrderDTO>>(tradeOrders);
        }

        public async Task CreateTradeOrder(TradeOrderDTO tradeOrderDto)
        {
            var tradeOrder = _mapper.Map<TradeOrder>(tradeOrderDto);
            tradeOrder.InsertByUser = 1;
            tradeOrder.InsertDate = DateTime.UtcNow;

            await _context.TradeOrder.AddAsync(tradeOrder);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTestTradeOrders(int tradeRuleId)
        {
            var tradeOrders = await _context.TradeOrder.Where(_ => _.TradeRuleId == tradeRuleId && _.IsTestOrder == true).ToArrayAsync();
            _context.TradeOrder.RemoveRange(tradeOrders);
            await _context.SaveChangesAsync();
        }
    }
}
