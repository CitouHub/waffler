using AutoMapper;

using WaffleBot.Data;
using WaffleBot.Data.ComplexModel;

namespace WaffleBot.Domain
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            SetupBitpandaMaps();

            CreateMap<CandleStickDTO, CandleStick>().ReverseMap();

            CreateMap<TradeRule, TradeRuleDTO>()
                .ForMember(dest => dest.TradeRuleConditions, opt => opt.MapFrom(src => src.TradeRuleCondition));
            CreateMap<TradeRuleCondition, TradeRuleConditionDTO>();
            CreateMap<sp_getPriceTrends_Result, PriceTrendsDTO>();
        }

        private void SetupBitpandaMaps()
        {
            CreateMap<Bitpanda.Public.CandleStickDTO, CandleStickDTO>()
                .ForMember(dest => dest.InstrumentCode, opt => opt.MapFrom(src => src.Instrument_Code))
                .ForMember(dest => dest.LastSequence, opt => opt.MapFrom(src => src.Last_Sequence))
                .ForMember(dest => dest.GranularityUnit, opt => opt.MapFrom(src => src.Granularity.Unit))
                .ForMember(dest => dest.GranularityPeriod, opt => opt.MapFrom(src => src.Granularity.Period))
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
