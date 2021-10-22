using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Waffler.Data;
using Waffler.Domain;

namespace Waffler.Service
{
    public interface ITradeOrderService
    {
        Task CreateTradeOrder(TradeOrderDTO tradeOrderDto);
        Task<List<TradeOrderDTO>> GetTradeOrders(DateTime from, DateTime to);
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
            var tradeOrders = await _context.TradeOrder.Where(_ => _.OrderDateTime >= from && _.OrderDateTime <= to).ToListAsync();
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
    }
}
