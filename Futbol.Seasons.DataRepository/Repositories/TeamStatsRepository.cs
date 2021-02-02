using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface ITeamStatsRepository : IRepository<TeamSeasonStats>
    {

    }
    public class TeamStatsRepository : Repository<TeamSeasonStats>, ITeamStatsRepository
    {
        private const string TeamsTableName = "Teams";
        public TeamStatsRepository(IConfiguration config) : base(config, TeamsTableName)
        {
        }
    }
}
