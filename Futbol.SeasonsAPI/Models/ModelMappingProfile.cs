using AutoMapper;
using Futbol.Seasons.BusinessEntities;

namespace Futbol.SeasonsAPI.Models
{
    public class ModelMappingProfile : Profile
    {
        public ModelMappingProfile()
        {
            #region Team

            CreateMap<TeamAddRequest, Team>()
                .ForMember(dest => dest.Abbreviation, opt => opt.MapFrom(src => src.Abbr));

            CreateMap<Team, TeamModel>();
            #endregion
            #region Match

            CreateMap<MatchAddRequest, Match>()
                .ForMember(dest => dest.Year,
                    opt => opt.MapFrom((src, dest, destMember, context) => (short) context.Items["year"]))
                .ForMember(dest => dest.Season,
                    opt => opt.MapFrom((src, dest, destMember, context) => (byte) context.Items["season"]))
                .ForMember(dest => dest.Round,
                    opt => opt.MapFrom((src, dest, destMember, context) => (byte) context.Items["round"]));

            #endregion

        }
    }
}
