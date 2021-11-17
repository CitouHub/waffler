using AutoMapper;
using System;
using Waffler.Data;
using Waffler.Data.ComplexModel;

namespace Waffler.Domain
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            SetupBitpandaMaps();

            CreateMap<CandleStickDTO, CandleStick>().ReverseMap();
            CreateMap<TradeRuleCondition, TradeRuleConditionDTO>()
                .ForMember(dest => dest.CandleStickValueTypeName, opt => opt.MapFrom(src => src.CandleStickValueType.Name))
                .ForMember(dest => dest.TradeRuleConditionComparatorName, opt => opt.MapFrom(src => src.TradeRuleConditionComparator.Name))
                .ForMember(dest => dest.TradeRuleConditionSampleDirectionName, opt => opt.MapFrom(src => src.TradeRuleConditionSampleDirection.Name))
                .ReverseMap();
            CreateMap<TradeOrder, TradeOrderDTO>().ReverseMap();
            CreateMap<TradeRule, TradeRuleDTO>()
                .ForMember(dest => dest.TradeActionName, opt => opt.MapFrom(src => src.TradeAction.Name))
                .ForMember(dest => dest.TradeTypeName, opt => opt.MapFrom(src => src.TradeType.Name))
                .ForMember(dest => dest.TradeConditionOperatorName, opt => opt.MapFrom(src => src.TradeConditionOperator.Name))
                .ForMember(dest => dest.TradeRuleConditions, opt => opt.MapFrom(src => src.TradeRuleCondition));

            CreateMap<sp_getCandleSticks_Result, CandleStickDTO>();
            CreateMap<sp_getTradeOrders_Result, TradeOrderDTO>();
        }

        private void SetupBitpandaMaps()
        {
            CreateMap<Bitpanda.Private.Balance.BalanceDTO, BalanceDTO>()
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => Common.Bitpanda.GetCurrentCode(src.currency_code)))
                .ForMember(dest => dest.Available, opt => opt.MapFrom(src => Math.Round(src.available, 4)))
                .ForMember(dest => dest.TradeLocked, opt => opt.MapFrom(src => Math.Round(src.locked, 4)));

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
        }
    }
}
