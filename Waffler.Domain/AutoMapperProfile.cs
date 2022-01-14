using AutoMapper;

using Waffler.Data;
using Waffler.Data.ComplexModel;
using Waffler.Domain.ComplexMapping;
using Waffler.Domain.Export;
using Waffler.Domain.Statistics;

namespace Waffler.Domain
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            SetupBitpandaMaps();

            CreateMap<ProfileDTO, WafflerProfile>().ReverseMap()
                .ForMember(dest => dest.ApiKey, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ApiKey) == false ? "[Your api key is set but will not be displayed here]" : null));
            CreateMap<CandleStickDTO, CandleStick>().ReverseMap();
            CreateMap<TradeRuleCondition, TradeRuleConditionDTO>()
                .ForMember(dest => dest.TradeRuleConditionComparatorName, opt => opt.MapFrom(src => src.TradeRuleConditionComparator != null ? src.TradeRuleConditionComparator.Name : null))
                .ForMember(dest => dest.FromCandleStickValueTypeName, opt => opt.MapFrom(src => src.FromCandleStickValueType != null ? src.FromCandleStickValueType.Name : null))
                .ForMember(dest => dest.FromTradeRuleConditionPeriodDirectionName, opt => opt.MapFrom(src => src.FromTradeRuleConditionPeriodDirection != null ? src.FromTradeRuleConditionPeriodDirection.Name : null))
                .ForMember(dest => dest.ToCandleStickValueTypeName, opt => opt.MapFrom(src => src.ToCandleStickValueType != null ? src.ToCandleStickValueType.Name : null))
                .ForMember(dest => dest.ToTradeRuleConditionPeriodDirectionName, opt => opt.MapFrom(src => src.ToTradeRuleConditionPeriodDirection != null ? src.ToTradeRuleConditionPeriodDirection.Name : null))
                .ReverseMap()
                .ForMember(dest => dest.TradeRuleConditionComparator, opt => opt.Ignore())
                .ForMember(dest => dest.FromCandleStickValueType, opt => opt.Ignore())
                .ForMember(dest => dest.FromTradeRuleConditionPeriodDirection, opt => opt.Ignore())
                .ForMember(dest => dest.ToCandleStickValueType, opt => opt.Ignore())
                .ForMember(dest => dest.ToTradeRuleConditionPeriodDirection, opt => opt.Ignore());

            CreateMap<TradeOrder, TradeOrderDTO>()
                .ForMember(dest => dest.TradeActionName, opt => opt.MapFrom(src => src.TradeAction != null ? src.TradeAction.Name : null))
                .ForMember(dest => dest.TradeOrderStatusName, opt => opt.MapFrom(src => src.TradeOrderStatus != null ? src.TradeOrderStatus.Name : null))
                .ForMember(dest => dest.TradeRuleId, opt => opt.MapFrom(src => src.TradeRuleId != null ? src.TradeRuleId : 0))
                .ForMember(dest => dest.TradeRuleName, opt => opt.MapFrom(src => TradeOrderMapper.GetTradeRuleName(src)))
                .ReverseMap()
                .ForMember(dest => dest.TradeAction, opt => opt.Ignore())
                .ForMember(dest => dest.TradeOrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRule, opt => opt.Ignore());
            CreateMap<TradeRule, TradeRuleDTO>()
                .ForMember(dest => dest.TradeActionName, opt => opt.MapFrom(src => src.TradeAction != null ? src.TradeAction.Name : null))
                .ForMember(dest => dest.TradeTypeName, opt => opt.MapFrom(src => src.TradeType != null ? src.TradeType.Name : null))
                .ForMember(dest => dest.TradeConditionOperatorName, opt => opt.MapFrom(src => src.TradeConditionOperator != null ? src.TradeConditionOperator.Name : null))
                .ForMember(dest => dest.TradeRuleStatusName, opt => opt.MapFrom(src => src.TradeRuleStatus != null ? src.TradeRuleStatus.Name : null))
                .ForMember(dest => dest.CandleStickValueTypeName, opt => opt.MapFrom(src => src.CandleStickValueType != null ? src.CandleStickValueType.Name : null))
                .ReverseMap()
                .ForMember(dest => dest.TradeAction, opt => opt.Ignore())
                .ForMember(dest => dest.TradeConditionOperator, opt => opt.Ignore())
                .ForMember(dest => dest.TradeOrders, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRuleConditions, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRuleStatus, opt => opt.Ignore())
                .ForMember(dest => dest.TradeType, opt => opt.Ignore())
                .ForMember(dest => dest.CandleStickValueType, opt => opt.Ignore());
            
            CreateMap<TradeRuleDTO, TradeRuleExportDTO>();
            CreateMap<TradeRuleConditionDTO, TradeRuleConditionExportDTO>();

            CreateMap<TradeAction, CommonAttributeDTO>();
            CreateMap<CandleStickValueType, CommonAttributeDTO>();
            CreateMap<TradeConditionOperator, CommonAttributeDTO>();
            CreateMap<TradeOrderStatus, CommonAttributeDTO>();
            CreateMap<TradeRuleStatus, CommonAttributeDTO>();
            CreateMap<TradeRuleCondition, CommonAttributeDTO>();
            CreateMap<TradeRuleConditionComparator, CommonAttributeDTO>();
            CreateMap<TradeRuleConditionPeriodDirection, CommonAttributeDTO>();
            CreateMap<TradeType, CommonAttributeDTO>();

            CreateMap<sp_getCandleSticks_Result, CandleStickDTO>();
            CreateMap<sp_getTradeRuleBuyStatistics_Result, TradeRuleBuyStatisticsDTO>()
                .ForMember(dest => dest.TradeRuleId, opt => opt.MapFrom(src => src.TradeRuleId != null ? src.TradeRuleId : 0))
                .ForMember(dest => dest.TradeRuleName, opt => opt.MapFrom(src => TradeRuleBuyStatisticsMapper.GetTradeRuleName(src)));
        }

        private void SetupBitpandaMaps()
        {
            CreateMap<Bitpanda.Private.Balance.BalanceDTO, BalanceDTO>()
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => Common.Bitpanda.GetCurrentCode(src.Currency_code)))
                .ForMember(dest => dest.Available, opt => opt.MapFrom(src => BalanceMapper.RoundBalance(src.Currency_code, src.Available)))
                .ForMember(dest => dest.TradeLocked, opt => opt.MapFrom(src => BalanceMapper.RoundBalance(src.Currency_code, src.Locked)));

            CreateMap<Bitpanda.Public.CandleStickDTO, CandleStickDTO>()
                .ForMember(dest => dest.TradeTypeId, opt => opt.MapFrom(src => (short)Common.Bitpanda.GetTradeType(src.Instrument_Code)))
                .ForMember(dest => dest.HighPrice, opt => opt.MapFrom(src => src.High))
                .ForMember(dest => dest.LowPrice, opt => opt.MapFrom(src => src.Low))
                .ForMember(dest => dest.OpenPrice, opt => opt.MapFrom(src => src.Open))
                .ForMember(dest => dest.ClosePrice, opt => opt.MapFrom(src => src.Close))
                .ForMember(dest => dest.PeriodDateTime, opt => opt.MapFrom(src => src.Time))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.Total_Amount));

            CreateMap<Bitpanda.Private.Order.OrderDTO, TradeOrderDTO>()
                .ForMember(dest => dest.TradeActionId, opt => opt.MapFrom(src => (short)Common.Bitpanda.GetTradeAction(src.Side)))
                .ForMember(dest => dest.TradeOrderStatusId, opt => opt.MapFrom(src => (short)Common.Bitpanda.GetTradeOrderStatus(src.Status)))
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Order_id))
                .ForMember(dest => dest.OrderDateTime, opt => opt.MapFrom(src => src.Time))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.FilledAmount, opt => opt.MapFrom(src => src.Filled_amount))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Common.Bitpanda.GetTradeOrderActive(src.Status)));
        }
    }
}
