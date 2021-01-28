using Futbol.Seasons.DataRepository.DataEntities;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository.Repositories
{
    public interface ITeamRepository : IRepository<Team>
    {

    }
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private const string TeamsTableName = "Teams";
        public TeamRepository(IConfiguration config) : base(config, TeamsTableName)
        {
        }
    }
}
