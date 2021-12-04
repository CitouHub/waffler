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
        Task AddTradeOrderAsync(TradeOrderDTO tradeOrderDto);
        Task<List<TradeOrderDTO>> GetTradeOrdersAsync(DateTime from, DateTime to);
        Task<List<TradeOrderDTO>> GetActiveTradeOrdersAsync();
        Task RemoveTestTradeOrdersAsync(int tradeRuleId);
        Task<TradeOrderDTO> GetLastTradeOrderAsync(DateTime toPeriodDateTime);
        Task<bool> UpdateTradeOrderAsync(TradeOrderDTO tradeOrdersDTO);
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

        public async Task<List<TradeOrderDTO>> GetTradeOrdersAsync(DateTime from, DateTime to)
        {
            var tradeOrders = await _context.sp_getTradeOrders(from, to);
            return _mapper.Map<List<TradeOrderDTO>>(tradeOrders);
        }

        public async Task<List<TradeOrderDTO>> GetActiveTradeOrdersAsync()
        {
            var tradeOrders = await _context.TradeOrders.Where(_ => _.IsTestOrder == false && _.IsActive == true).ToListAsync();
            return _mapper.Map<List<TradeOrderDTO>>(tradeOrders);
        }

        public async Task AddTradeOrderAsync(TradeOrderDTO tradeOrderDto)
        {
            var tradeOrder = _mapper.Map<TradeOrder>(tradeOrderDto);
            tradeOrder.InsertByUser = 1;
            tradeOrder.InsertDate = DateTime.UtcNow;

            await _context.TradeOrders.AddAsync(tradeOrder);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveTestTradeOrdersAsync(int tradeRuleId)
        {
            var tradeOrders = await _context.TradeOrders.Where(_ => _.TradeRuleId == tradeRuleId && _.IsTestOrder == true).ToArrayAsync();
            _context.TradeOrders.RemoveRange(tradeOrders);
            await _context.SaveChangesAsync();
        }

        public async Task<TradeOrderDTO> GetLastTradeOrderAsync(DateTime toOrderDateTime)
        {
            var tradeOrder = await _context.TradeOrders.Where(_ => _.OrderDateTime <= toOrderDateTime && _.IsTestOrder == false)
                .OrderByDescending(_ => _.OrderDateTime).FirstOrDefaultAsync();
            return _mapper.Map<TradeOrderDTO>(tradeOrder);
        }

        public async Task<bool> UpdateTradeOrderAsync(TradeOrderDTO tradeOrdersDTO)
        {
            var tradeOrder = await _context.TradeOrders.FindAsync(tradeOrdersDTO.Id);
            if (tradeOrder != null)
            {
                _mapper.Map(tradeOrdersDTO, tradeOrder);
                tradeOrder.UpdateByUser = 1;
                tradeOrder.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
