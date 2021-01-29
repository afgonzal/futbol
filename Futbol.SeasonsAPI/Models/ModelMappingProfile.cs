using AutoMapper;
using Futbol.Seasons.BusinessEntities;

namespace Futbol.SeasonsAPI.Models
{
    public class ModelMappingProfile : Profile
    {
        public ModelMappingProfile()
        {
            CreateMap<TeamAddRequest, Team>();

            CreateMap<Team, TeamModel>();
        }
    }
}
