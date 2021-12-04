using System;

using AutoMapper;

using Waffler.Data;
using Waffler.Data.ComplexModel;

namespace Waffler.Domain
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            SetupBitpandaMaps();

            CreateMap<ProfileDTO, WafflerProfile>().ReverseMap();
            CreateMap<CandleStickDTO, CandleStick>().ReverseMap();
            CreateMap<TradeRuleCondition, TradeRuleConditionDTO>()
                .ForMember(dest => dest.CandleStickValueTypeName, opt => opt.MapFrom(src => src.CandleStickValueType.Name))
                .ForMember(dest => dest.TradeRuleConditionComparatorName, opt => opt.MapFrom(src => src.TradeRuleConditionComparator.Name))
                .ForMember(dest => dest.TradeRuleConditionSampleDirectionName, opt => opt.MapFrom(src => src.TradeRuleConditionSampleDirection.Name))
                .ReverseMap()
                .ForMember(dest => dest.CandleStickValueType, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRuleConditionComparator, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRuleConditionSampleDirection, opt => opt.Ignore());
            CreateMap<TradeOrder, TradeOrderDTO>()
                .ReverseMap()
                .ForMember(dest => dest.TradeAction, opt => opt.Ignore())
                .ForMember(dest => dest.TradeOrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRule, opt => opt.Ignore());
            CreateMap<TradeRule, TradeRuleDTO>()
                .ForMember(dest => dest.TradeActionName, opt => opt.MapFrom(src => src.TradeAction.Name))
                .ForMember(dest => dest.TradeTypeName, opt => opt.MapFrom(src => src.TradeType.Name))
                .ForMember(dest => dest.TradeConditionOperatorName, opt => opt.MapFrom(src => src.TradeConditionOperator.Name))
                .ForMember(dest => dest.TradeRuleStatusName, opt => opt.MapFrom(src => src.TradeRuleStatus.Name))
                .ForMember(dest => dest.TradeRuleConditions, opt => opt.MapFrom(src => src.TradeRuleConditions))
                .ReverseMap()
                .ForMember(dest => dest.TradeAction, opt => opt.Ignore())
                .ForMember(dest => dest.TradeConditionOperator, opt => opt.Ignore())
                .ForMember(dest => dest.TradeOrders, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRuleConditions, opt => opt.Ignore())
                .ForMember(dest => dest.TradeRuleStatus, opt => opt.Ignore())
                .ForMember(dest => dest.TradeType, opt => opt.Ignore());

            CreateMap<TradeAction, CommonAttributeDTO>();
            CreateMap<CandleStickValueType, CommonAttributeDTO>();
            CreateMap<TradeConditionOperator, CommonAttributeDTO>();
            CreateMap<TradeOrderStatus, CommonAttributeDTO>();
            CreateMap<TradeRuleStatus, CommonAttributeDTO>();
            CreateMap<TradeRuleCondition, CommonAttributeDTO>();
            CreateMap<TradeRuleConditionComparator, CommonAttributeDTO>();
            CreateMap<TradeRuleConditionSampleDirection, CommonAttributeDTO>();
            CreateMap<TradeType, CommonAttributeDTO>();

            CreateMap<sp_getCandleSticks_Result, CandleStickDTO>();
            CreateMap<sp_getTradeOrders_Result, TradeOrderDTO>();
        }

        private void SetupBitpandaMaps()
        {
            CreateMap<Bitpanda.Private.Balance.BalanceDTO, BalanceDTO>()
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => Common.Bitpanda.GetCurrentCode(src.Currency_code)))
                .ForMember(dest => dest.Available, opt => opt.MapFrom(src => Math.Round(src.Available, 4)))
                .ForMember(dest => dest.TradeLocked, opt => opt.MapFrom(src => Math.Round(src.Locked, 4)));

            CreateMap<Bitpanda.Public.CandleStickDTO, CandleStickDTO>()
                .ForMember(dest => dest.TradeTypeId, opt => opt.MapFrom(src => (short)Common.Bitpanda.GetTradeType(src.Instrument_Code)))
                .ForMember(dest => dest.HighPrice, opt => opt.MapFrom(src => src.High))
                .ForMember(dest => dest.LowPrice, opt => opt.MapFrom(src => src.Low))
                .ForMember(dest => dest.OpenPrice, opt => opt.MapFrom(src => src.Open))
                .ForMember(dest => dest.ClosePrice, opt => opt.MapFrom(src => src.Close))
                .ForMember(dest => dest.AvgHighLowPrice, opt => opt.MapFrom(src => (src.High + src.Low) / 2))
                .ForMember(dest => dest.AvgOpenClosePrice, opt => opt.MapFrom(src => (src.Open + src.Close) / 2))
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
