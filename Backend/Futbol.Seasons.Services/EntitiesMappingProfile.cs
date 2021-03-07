using System.Diagnostics;
using AutoMapper;
using Futbol.Seasons.BusinessEntities;

namespace Futbol.Seasons.Services
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile()
        {
            #region Team
            CreateMap<DataRepository.DataEntities.TeamProfile, Team>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TeamName));

            CreateMap<Team, DataRepository.DataEntities.TeamProfile>()
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Name));

            #endregion

            #region Match

            CreateMap<DataRepository.DataEntities.Match, Match>();


            CreateMap<Match, DataRepository.DataEntities.Match>()
                .ForMember(dest => dest.YearSeasonRound, opt => opt.MapFrom(src => $"{src.Year}#{src.Season}#{src.Round}"));
            #endregion

            #region TeamStats 

            CreateMap<DataRepository.DataEntities.TeamSeasonStats, TeamStats>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TeamName));


            CreateMap<TeamStats, DataRepository.DataEntities.TeamSeasonStats>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Name))
                .ForCtorParam("year", opt => opt.MapFrom((src, ctx) => ctx.Items["year"]))
                .ForCtorParam("season", opt => opt.MapFrom((src, ctx) => ctx.Items["season"]))
                .ForCtorParam("teamId", opt => opt.MapFrom((src => src.Id)));

            #endregion

            #region Config

            CreateMap<DataRepository.DataEntities.SeasonConfig, SeasonConfig>();
            CreateMap<DataRepository.DataEntities.ChampionshipConfig, ChampionshipConfig>();

            CreateMap<DataRepository.DataEntities.CurrentSeasonConfig, CurrentSeasonConfig>();

            #endregion

            #region Partner

            CreateMap<DataRepository.DataEntities.User, User>();

            #endregion

        }
    }
}
