using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waffler.Data;
using Waffler.Domain;

namespace Waffler.Service
{
    public interface ITradeOrderService
    {
        public Task CreateTradeOrder(TradeOrderDTO tradeOrderDto);
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
