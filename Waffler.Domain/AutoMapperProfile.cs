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

            CreateMap<CandleStickDTO, CandleStick>().ReverseMap();
            CreateMap<TradeRuleCondition, TradeRuleConditionDTO>().ReverseMap();
            CreateMap<TradeOrder, TradeOrderDTO>().ReverseMap();
            CreateMap<TradeRule, TradeRuleDTO>()
                .ForMember(dest => dest.TradeRuleConditions, opt => opt.MapFrom(src => src.TradeRuleCondition));

            CreateMap<sp_getCandleSticks_Result, CandleStickDTO>();
        }

        private void SetupBitpandaMaps()
        {
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
