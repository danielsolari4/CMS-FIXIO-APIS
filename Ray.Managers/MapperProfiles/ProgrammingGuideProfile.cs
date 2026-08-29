using AutoMapper;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;

namespace Ray.Managers.MapperProfiles
{
    internal class ProgrammingGuideProfile : Profile
    {
        public ProgrammingGuideProfile()
        {
            CreateMap<ProgrammingGuide, ProgrammingGuideDto>()
                .ForMember(x => x.Nodes, opt => opt.Ignore())
                .ForMember(x => x.Media, opt => opt.Ignore())
                .ForMember(x => x.Node, opt => opt.Ignore())
                .ForMember(x => x.Schedule, opt => opt.Ignore());

            CreateMap<ProgrammingGuide, DeleteProgrammingGuideDtoBindingModel>()
                .ForMember(x => x.Nodes, opt => opt.Ignore())
                .ForMember(x => x.Media, opt => opt.Ignore())
                .ForMember(x => x.Node, opt => opt.Ignore())
                .ForMember(x => x.Schedule, opt => opt.Ignore());

            var m1 = CreateMap<ProgrammingGuideDto, ProgrammingGuide>();
                m1.ForAllMembers(x => x.Ignore());
                m1.ForMember(x => x.ProgramName, opt => opt.MapFrom(src => src.ProgramName))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(x => x.Channel, opt => opt.MapFrom(src => src.ChannelId))
                .ForMember(x => x.SecondaryUrl, opt => opt.MapFrom(src => src.SecondaryUrl))
                .ForMember(x => x.MobileUrl, opt => opt.MapFrom(src => src.MobileUrl))
                .ForMember(x => x.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled));

            CreateMap<ProgrammingGuideSchedule, ProgrammingGuideScheduleDto>();

            var m2 = CreateMap<ProgrammingGuideScheduleDto, ProgrammingGuideSchedule>();
                m2.ForAllMembers(x => x.Ignore());
                m2.ForMember(x => x.InitHour, opt => opt.MapFrom(src => src.InitHour))
                .ForMember(x => x.InitMinute, opt => opt.MapFrom(src => src.InitMinute))
                .ForMember(x => x.EndHour, opt => opt.MapFrom(src => src.EndHour))
                .ForMember(x => x.EndMinute, opt => opt.MapFrom(src => src.EndMinute))
                .ForMember(x => x.Day, opt => opt.MapFrom(src => src.Day));
        }
    }
}
