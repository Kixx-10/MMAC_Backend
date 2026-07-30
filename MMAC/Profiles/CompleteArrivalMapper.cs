using AutoMapper;
using MMAC.DTOS;
using MMAC.Models.Cores;

namespace MMAC.Profiles
{
    public class CompleteArrivalMapper : Profile
    {
        public CompleteArrivalMapper()
        {
            CreateMap<ArrivalApplication, ResponseCompleteArrivalDTO>()
                .ForMember(dest => dest.TravellerId, opt => opt.MapFrom(src => src.TravellerId));
            CreateMap<CompleteArrivalDTO, Traveller>().ReverseMap();
            CreateMap<CompleteArrivalDTO, ArrivalApplication>().ReverseMap();
            CreateMap<ResponseCompleteArrivalDTO, Traveller>().ReverseMap();
            CreateMap<ResponseCompleteArrivalDTO, ArrivalApplication>().ReverseMap();
            CreateMap<ArrivalApplication, ResponseForeignerArrivalDTO>()
                .ForMember(dest => dest.HealthRecordUrl, opt => opt.MapFrom(src => src.HealthRecordUrl))
                .ReverseMap();

            CreateMap<Traveller, ResponseForeignerArrivalDTO>()
                .ReverseMap();

            CreateMap<ArrivalApplication, ResponseMyanmarArrivalDTO>()
                .ForMember(dest => dest.HealthRecordUrl, opt => opt.MapFrom(src => src.HealthRecordUrl))
                .ReverseMap();

            CreateMap<Traveller, ResponseMyanmarArrivalDTO>()
                .ReverseMap();
        }
    }
}
