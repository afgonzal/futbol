using System.Diagnostics;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;

namespace Futbol.Seasons.Services
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile()
        {
            CreateMap<DataRepository.DataEntities.Team, Team>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TeamName));

            CreateMap<Team, DataRepository.DataEntities.Team>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Name));
        }
    }
}
