using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using AutoMapper;

using Waffler.Data;
using Waffler.Domain;
using Waffler.Common;

namespace Waffler.Service
{
    public interface ITradeOrderService
    {
        Task AddTradeOrderAsync(TradeOrderDTO tradeOrderDto);
        Task<List<TradeOrderDTO>> GetTradeOrdersAsync(DateTime from, DateTime to);
        Task<List<TradeOrderDTO>> GetActiveTradeOrdersAsync();
        Task<int> RemoveTestTradeOrdersAsync(int tradeRuleId);
        Task<bool> UpdateTradeOrderAsync(TradeOrderDTO tradeOrdersDTO);
        Task<bool> SetTradeRuleAsync(int tradeOrderId, int tradeRuleId);
        Task<IEnumerable<CommonAttributeDTO>> GetTradeOrderStatusesAsync();
        Task<bool> AnyTradeOrdersAsync(int tradeRuleId);
        Task DeleteTestTradeOrdersAsync(int tradeRuleId);
        Task<bool> TradeOrderExistsAsync(Guid orderId);
        Task<DateTime?> GetTradeOrderSyncPositionAsync();
        Task SetTradeOrderSyncPositionAsync(DateTime position);
    }

    public class TradeOrderService : ITradeOrderService
    {
        private readonly ILogger<TradeOrderService> _logger;
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;

        public TradeOrderService(ILogger<TradeOrderService> logger, WafflerDbContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _logger.LogDebug("TradeOrderService instantiated");
        }

        public async Task<List<TradeOrderDTO>> GetTradeOrdersAsync(DateTime from, DateTime to)
        {
            var tradeOrders = await _context.TradeOrders
                .Include(_ => _.TradeRule)
                .Include(_ => _.TradeAction)
                .Include(_ => _.TradeOrderStatus)
                .Where(_ => _.OrderDateTime >= from && _.OrderDateTime <= to).ToArrayAsync();
            return _mapper.Map<List<TradeOrderDTO>>(tradeOrders);
        }

        public async Task<List<TradeOrderDTO>> GetActiveTradeOrdersAsync()
        {
            var tradeOrders = await _context.TradeOrders.Where(_ => 
                _.TradeOrderStatusId != (short)Variable.TradeOrderStatus.Test && 
                _.IsActive == true).ToListAsync();
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

        public async Task<int> RemoveTestTradeOrdersAsync(int tradeRuleId)
        {
            var tradeOrders = await _context.TradeOrders.Where(_ => 
                _.TradeRuleId == tradeRuleId &&
                _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test).ToArrayAsync();
            _context.TradeOrders.RemoveRange(tradeOrders);
            await _context.SaveChangesAsync();

            return tradeOrders.Count();
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

        public async Task<bool> SetTradeRuleAsync(int tradeOrderId, int tradeRuleId)
        {
            var tradeOrder = await _context.TradeOrders.FindAsync(tradeOrderId);
            if (tradeOrder != null && tradeOrder.TradeRuleId == null)
            {
                if(_context.TradeRules.Any(_ => _.Id == tradeRuleId))
                {
                    tradeOrder.TradeRuleId = tradeRuleId;
                    tradeOrder.UpdateByUser = 1;
                    tradeOrder.UpdateDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        public async Task<IEnumerable<CommonAttributeDTO>> GetTradeOrderStatusesAsync()
        {
            var tradeOrderStatuses = await _context.TradeOrderStatuses.ToArrayAsync();
            return _mapper.Map<List<CommonAttributeDTO>>(tradeOrderStatuses);
        }

        public async Task<bool> AnyTradeOrdersAsync(int tradeRuleId)
        {
            return await _context.TradeOrders.AnyAsync(_ => 
                _.TradeRuleId == tradeRuleId && 
                _.TradeOrderStatusId != (short)Variable.TradeOrderStatus.Test);
        }

        public async Task DeleteTestTradeOrdersAsync(int tradeRuleId)
        {
            var testTradeOrders = await _context.TradeOrders.Where(_ =>
                _.TradeRuleId == tradeRuleId &&
                _.TradeOrderStatusId == (short)Variable.TradeOrderStatus.Test).ToListAsync();

            if(testTradeOrders.Any())
            {
                _context.TradeOrders.RemoveRange(testTradeOrders);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> TradeOrderExistsAsync(Guid orderId)
        {
            return await _context.TradeOrders.AnyAsync(_ => _.OrderId == orderId);
        }

        public async Task<DateTime?> GetTradeOrderSyncPositionAsync()
        {
            return (await _context.TradeOrderSyncStatuses.FirstOrDefaultAsync())?.CurrentPosition;
        }

        public async Task SetTradeOrderSyncPositionAsync(DateTime position)
        {
            var tradeOrderSyncStatus = await _context.TradeOrderSyncStatuses.FirstOrDefaultAsync();
            if(tradeOrderSyncStatus != null)
            {
                if(position > DateTime.UtcNow)
                {
                    position = DateTime.UtcNow;
                }

                tradeOrderSyncStatus.CurrentPosition = position;
                tradeOrderSyncStatus.UpdateByUser = 1;
                tradeOrderSyncStatus.UpdateDate = DateTime.UtcNow;
            } 
            else
            {
                await _context.TradeOrderSyncStatuses.AddAsync(new TradeOrderSyncStatus()
                {
                    CurrentPosition = position,
                    InsertByUser = 1,
                    InsertDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
